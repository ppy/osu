#!/bin/bash
echo "Installing Cake.Tool..."
dotnet tool restore

# Parse arguments.
CAKE_ARGUMENTS=()
for i in "$@"; do
    case $1 in
        -s|--script) SCRIPT="$2"; shift ;;
        --) shift; CAKE_ARGUMENTS+=("$@"); break ;;
        deb) shift; build_deb=1; break ;;
        debonly) shift; build_deb=1; debonly=1; break ;;
        *) CAKE_ARGUMENTS+=("$1") ;;
    esac
    shift
done

if [ ! "$debonly" == "1" ];then
    echo "Running build script..."
    dotnet cake ./build/build.cake --bootstrap
    dotnet cake ./build/build.cake "${CAKE_ARGUMENTS[@]}"
fi

if [ "$build_deb" == "1" ];then
    rm deb/*.deb
    builds=$(whiptail --title "Build list" --checklist \
    "Choose packages to build\n[Space]Select , [Enter]Confirm , [Esc]Cancel" 0 0 0 \
    "osu" "osu!lazer Chinese version" OFF \
    "avlibfix" "Fix video issues on Ubuntu19.10" OFF 3>&1 1>&2 2>&3)
    exitstatus=$?
    if [ $exitstatus = 0 ]; then
        if [ -z "$builds" ];then
            echo "Abort";
            exit 0;
        fi
        ./build-deb.sh "$builds"
    else
        echo "You chose Cancel."
        exit 0
    fi
    if [ "$?" == "0" ];then
        option=$(whiptail --inputbox "Built packages:\n============\n`ls deb | grep .deb`\n============\nInstall them now? [y/n | default:n]" "" ""  3>&1 1>&2 2>&3);
            if [ "$option" == "y" ];then
                sudo dpkg -i ./deb/*.deb
            else
                echo "Abort"
            fi
    fi
fi