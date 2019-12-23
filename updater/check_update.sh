#!/bin/bash
readonly VERSION_PREFIX="2019.1113.0+matrixfeather"
readonly VERSION=`sed -n '1,1p' ./VERSION`
readonly VERSIONURL="localhost:4000/mf-osu/VERSION"
#readonly VERSIONURL="https://raw.githubusercontent.com/MATRIX-feather/osu/lang_zhCN/updater/VERSION"
readonly update_warn="\n请注意!大陆地区推荐科学上网,从gitee下载的功能将在后续几次更新内加入"

wget "$VERSIONURL" -O VERSION.get
if [ $? != 0 ];then
            zenity --error --title="出现问题" --text="获取更新时出现问题" --modal
            rm ./VERSION.get
            exit 1;
fi

get_version=`sed -n '1,1p' ./VERSION.get`
get_release=`sed -n '2,2p' ./VERSION.get`
DownloadUpdate(){
        update_url=`sed -n '3,3p' ./VERSION.get`
        wget "$update_url" --limit-rate=20000k -c 2>&1 | \
        sed -u 's/^.* \+\([0-9]\+%\) \+\([0-9.]\+[GMKB]\) \+\([0-9hms.]\+\).*$/\1\n#下载中...(\1) \\n预计剩余时间:\3/'  |\
        zenity --progress --title="下载更新" --auto-close #本条指令修改自http://www.newsmth.net/nForum/#!article/Ubuntu/10628?p=1
}
VerifyUpdate(){
        md5sum_url=`sed -n '4,4p' ./VERSION.get`
        md5sum_get=`md5sum patch.zip|cut -d ' ' -f1`
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
        unzip -o ./patch.zip -d ./update
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
        zenity --question --title="发现重大更新" --text="发现mf-osu有重大更新,是否升级?\n最新版本:($get_release)\n你的版本:($VERSION_PREFIX$VERSION)\n$update_warn"
    else
        zenity --question --title="发现新版本" --text="发现mf-osu有新版本,是否更新?\n最新版本:($VERSION_PREFIX$get_version)\n你的版本:($VERSION_PREFIX$VERSION)\n$update_warn"
    fi
else
    exit 0
fi
if [ $? == 0 ];then
    while(true);do
        echo 25
        if [ -f "patch.zip" ];then
            VerifyUpdate;
            if [ $? != 0 ];then
                echo "#本地文件验证不通过!将重新下载"
                sleep 1
                rm ./patch.zip
                continue;
            fi
        else
            echo "#将在新窗口中开启下载"
            sleep 0.1
            for ((time=1;time<4;time++));do
                echo "#下载更新:第$time次尝试"
                DownloadUpdate;
                if [ $? == 0 ];then
                    break;
                fi
            done
        fi
        echo "#验证更新中";
        echo 50
        VerifyUpdate;
        if [ $? == 0 ];then
            zenity --question --title="现在安装?" --text="文件已经下载好了,是否现在安装?" --modal
        else 
            rm patch.zip
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
                    rm ./patch.zip
                    echo 100;
                    ;;
                2)
                    echo "#安装已完成!"
                    rm ./patch.zip
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
    done | zenity --progress --title="更新中.." --no-cancel --pulsate
fi