pushd /home/fabio.curi/Unity/Hub/Editor/2022.1.7f1/Editor
./Unity -batchmode -quit -projectPath "/home/fabio.curi/desarr/AgeOfJoy-2022.1/" -buildTarget Android -executeMethod AndroidBuilder.Build -logfile "/home/fabio.curi/desarr/AgeOfJoy-2022.1/build.log"
ll *.apk
popd