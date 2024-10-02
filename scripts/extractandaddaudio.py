import os
import zipfile
import yaml
import subprocess
from shutil import copy2

# Folder containing zip files
folder_path = '/mnt/d/AgeOfJoyCabinets'
# Output folder for modified zip files
output_folder = './output'

def extract_audio_from_video(video_path, output_audio_path):
    """Uses ffmpeg to extract audio from the video and save it as an MP3 file."""
    print(f"  Extracting audio from video {video_path} to {output_audio_path}")
    try:
        subprocess.run(['ffmpeg', '-i', video_path, '-q:a', '0', '-map', 'a', output_audio_path], check=True)
        print(f"  Audio extracted and saved as {output_audio_path}")
    except subprocess.CalledProcessError as e:
        print(f"  Error extracting audio: {e}")

def process_zip(zip_path, output_path):
    # Name of the modified zip in the output folder
    temp_zip_path = os.path.join(output_path, os.path.basename(zip_path))
    
    print(f"Processing zip file: {zip_path}")
    
    try:
        with zipfile.ZipFile(zip_path, 'r') as zip_ref:
            # Check if 'description.yaml' exists in the zip
            if 'description.yaml' not in zip_ref.namelist():
                print(f"  'description.yaml' not found in {zip_path}. Skipping.")
                return
            
            # Extract 'description.yaml'
            yaml_data = None
            with zip_ref.open('description.yaml') as yaml_file:
                yaml_data = yaml.safe_load(yaml_file)
            
            # Check for 'video' key and ensure it has a valid video file
            video_file = None
            if yaml_data and 'video' in yaml_data and 'file' in yaml_data['video']:
                video_file = yaml_data['video']['file']
                if video_file.endswith(('.mkv', '.mp4')):
                    print(f"  Found video file: {video_file} in {zip_path}")
                else:
                    print(f"  No valid video file in {zip_path}. Skipping.")
                    return
            else:
                print(f"  No valid 'video' entry in {zip_path}. Skipping.")
                return

            # Extract the video from the zip
            with zip_ref.open(video_file) as vf:
                video_data = vf.read()
                video_path = os.path.join(output_path, video_file)
                with open(video_path, 'wb') as video_out:
                    video_out.write(video_data)
            
            # Extract audio from the video
            audio_file = video_file.rsplit('.', 1)[0] + '.mp3'
            audio_path = os.path.join(output_path, audio_file)
            extract_audio_from_video(video_path, audio_path)

            # Add the 'audio-file' entry to the yaml
            if 'audio-file' not in yaml_data['video']:
                yaml_data['video']['audio-file'] = audio_file
                print(f"  Added 'audio-file' entry: {audio_file}")
            
            # Write the updated yaml content to a temporary file
            updated_yaml_path = os.path.join(output_path, 'updated_description.yaml')
            with open(updated_yaml_path, 'w') as updated_yaml_file:
                yaml.dump(yaml_data, updated_yaml_file)
            
            # Create a new zip file in the output folder, copying all contents except the old description.yaml
            with zipfile.ZipFile(temp_zip_path, 'w') as new_zip:
                print(f"  Creating new zip file: {temp_zip_path}")
                for item in zip_ref.infolist():
                    if item.filename != 'description.yaml':
                        new_zip.writestr(item, zip_ref.read(item.filename))
                
                # Add the modified description.yaml
                new_zip.write(updated_yaml_path, 'description.yaml')

                # Add the extracted audio file
                new_zip.write(audio_path, audio_file)
            
            # Clean up temporary files
            os.remove(video_path)
            os.remove(audio_path)
            os.remove(updated_yaml_path)
            print(f"  Finished processing {zip_path}. Modified zip saved as {temp_zip_path}.")

    except Exception as e:
        print(f"  Error processing {zip_path}: {e}")

def scan_folder(folder_path, output_path):
    # Ensure the output folder exists
    if not os.path.exists(output_path):
        os.makedirs(output_path)
        print(f"Output folder created: {output_path}")

    print(f"Scanning folder for zip files: {folder_path}")
    
    # Scan the folder for zip files
    for file_name in os.listdir(folder_path):
        if file_name.endswith('.zip'):
            zip_path = os.path.join(folder_path, file_name)
            process_zip(zip_path, output_path)

if __name__ == '__main__':
    scan_folder(folder_path, output_folder)
