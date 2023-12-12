from PIL import Image, ImageDraw, ImageFont

def add_number_to_image(input_path, output_path, number):
  # Open the image
    img = Image.open(input_path)

    # Get the side length of the square image
    side_length = min(img.size)

    # Calculate the desired font size (1/2 of the side length)
    font_size = side_length // 2

    # Create a drawing object
    draw = ImageDraw.Draw(img)

    # Choose the Verdana bold font
    font = ImageFont.truetype("verdanab.ttf", font_size)

    # Calculate text size and position
    text = str(number)
    text_width, text_height = draw.textbbox((0, 0), text, font)[2:]
    
    # Calculate ascent and descent
    ascent, descent = font.getmetrics()
    
    # Calculate the position to center the number
    x = (img.width - text_width) // 2
    y = (img.height - (text_height + descent)) // 2

    # Draw the number at the center
    draw.text((x, y), text, fill=(255, 255, 255), font=font)

    # Save the modified image
    img.save(output_path)

if __name__ == "__main__":
    # Generate 100 images
    for i in range(100):
        input_image_path = "C:/Users/curif/Downloads/AgentPlayerPosition.png"
        output_image_path = f"C:/Users/curif/Downloads/AgentPlayerPosition_{i}.png"
        number_to_draw = i

        add_number_to_image(input_image_path, output_image_path, number_to_draw)

        print(f"Image with number {number_to_draw} saved to {output_image_path}")
