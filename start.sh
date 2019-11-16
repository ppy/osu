#!/bin/bash

export GTK_IM_MODULE=fcitx  
export QT_IM_MODULE=fcitx  
export XMODIFIERS="@im=fcitx" 

cd /data/osu/

script_path="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
app_path="$script_path"/osu.Desktop/bin/Release/netcoreapp3.0

LD_LIBRARY_PATH="$app_path" dotnet "$app_path"/osu\!.dll --help
