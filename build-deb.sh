#!/bin/bash
while(true);do
    case $1 in
        "\"osu\"")
            osu=1;
            shift;
            ;;
        "\"avlibfix\"")
            av=1;
            shift;
            ;;
        "\"osu\" \"avlibfix\"")
            osu=1;
            av=1;
            shift;
            ;;
        "osu")
            osu=1;
            shift;
            ;;
        "avlibfix")
            av=1;
            shift;
            ;;
        *)
            break;
            ;;
    esac
done
if [ "$osu" == "1" ];then
echo "Building:osu..."
    cp ./osu.Desktop/bin/Release/netcoreapp3.0/* ./deb/osu/opt/matrixfeather-osu/ -r
    cp ./assets/lazer.png ./deb/osu/opt/matrixfeather-osu/icon.png
    dpkg -b deb/osu
fi
if [ "$av" == "1" ];then
echo "Building:avlibfix..."
    dpkg -b deb/avlibfix
fi