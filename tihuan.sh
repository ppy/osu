#!/bin/bash
cols=`stty size|awk '{print $2}'`
pause(){
  read -p "按下Enter键继续"
}

while(true);do
ori_arg=$(whiptail --title "原消息" --inputbox "" "$lines" "$cols"  3>&1 1>&2 2>&3)
if [ $? != 0 ];then 
exit 1;
fi
new_arg=$(whiptail --title "新消息" --inputbox "要替换的消息:\n$ori_arg" "$lines" "$cols"  3>&1 1>&2 2>&3)
sed -i "s/$ori_arg/$new_arg/g" ./osu.Desktop/bin/Release/netcoreapp3.0/Humanizer.dll
pause
done