#!/bin/bash
readonly VERSION_PREFIX="2019.1113.0+matrixfeather"
readonly VERSION=`sed -n '1,1p' ./VERSION`
readonly GETURL="https://github.com/MATRIX-feather/osu/releases/download/2019.1113.0%2Bmatrixfeather7"
wget "$GETURL/VERSION" -O VERSION.get
if [ $? != 0 ];then
            zenity --error --title="出现问题" --text="获取更新时出现问题" --modal
            rm ./VERSION.get
            exit 1;
fi
get_version=`sed -n '1,1p' ./VERSION.get`
get_release=`sed -n '2,2p' ./VERSION.get`
DownloadUpdate(){
        update_url=`sed -n '3,3p' ./VERSION.get`
        wget "$update_url"
}
VerifyUpdate(){
        md5sum_url=`sed -n '4,4p' ./VERSION.get`
        md5sum_get=`md5sum patch|cut -d ' ' -f1`
        if [ "$md5sum_url" == "$md5sum_get" ];then
            return 0
        else
            return 1
        fi
}
CleanArea(){
        rm ./VERSION.get;
        rm ./update/*;
        rmdir ./update;
}
InstallUpdate(){
        echo "#正在安装...\n解压文件至:$PWD/update"
        echo 60
        mkdir ./update
        unzip -o ./patch -d ./update
        echo "#正在安装...\n运行安装脚本"
        echo 75
        ./update/main.sh
        if [ $? != 0 ];then
            return 1;
            break;
        fi
        echo "#正在安装...\n显示变更列表"
        echo 90
        zenity --text-info --filename=./update/CHANGELOG --checkbox="阅读完后勾选该项"
        if [ $? != 0 ];then
            return 2;
            break;
        fi
}

if [ "$get_version" != "$VERSION" ];then
    if [ ! -z "$get_release" ];then
        zenity --question --title="发现重大更新" --text="发现mf-osu有重大更新,是否升级?\n最新版本:($get_release)\n你的版本:($VERSION_PREFIX$VERSION)"
    else
        zenity --question --title="发现新版本" --text="发现mf-osu有新版本,是否更新?\n最新版本:($VERSION_PREFIX$get_version)\n你的版本:($VERSION_PREFIX$VERSION)"
    fi
else
    exit 0
fi
if [ $? == 0 ];then
    while(true);do
        echo "#下载更新中";
        echo 25
        if [ -f "patch" ];then
            echo "#发现本地文件,将跳过下载"
            sleep 1
        else
            DownloadUpdate;
        fi
        echo "#验证更新中";
        echo 50
        VerifyUpdate;
        if [ $? == 0 ];then
            zenity --question --title="现在安装?" --text="文件已经下载好了,是否现在安装?" --modal
        else 
            rm patch
            zenity --error --title="出现问题" --text="
文件的MD5和预期中的不一样,请重新启动更新脚本进行下载\n\
预期MD5:\n$md5sum_url\n\
文件MD5:\n$md5sum_get" --modal
            return 1;
        fi
        if [ $? == 0 ];then
            InstallUpdate;
            case $? in
                0)
                    echo "#安装已完成!\n另外:感谢阅读变更列表!";
                    echo 100;
                    ;;
                2)
                    echo "#安装已完成!"
                    echo 100;
                    ;;
                *)
                    echo "#安装遇到问题!将取消升级\n安装时出现问题";
                    CleanArea;
            esac
        else
            echo "#安装遇到问题!将取消升级\n文件不匹配或取消安装"
            CleanArea;
        fi
        CleanArea;
        break;
    done | zenity --progress --title="更新中.." --no-cancel
fi