# 本体DBus API

## io.matrix_feather.dbus.greet

### 描述
用于确认DBus连接是否正常，以及向游戏内发送消息

### 函数、参数、以及返回值

* `Greet`
    - 确认DBus连接是否正常

    - 参数:
        - `string s1`: 发送方的名字

    - 返回:
        - `string`: 可能是`Greetings from xxx: xxxx , {s1}!`

* `SendMessage`
    - 向游戏内发送信息

    - 参数:
        - `string s1`: 要发送的消息

    - 返回:
        - `bool`: 玩家是否允许此功能的使用
    
    - 注:
        - 当`s1`为空时，游戏内不会显示任何通知也不会记录任何东西
        - 通过此接口发送的消息会被游戏记录到日志中


## io.matrix_feather.dbus.Audio

### 描述
用于获取当前音频的进度以及长度

### 函数、参数、以及返回值

* `GetTrackLength`
    - 获取当前音频的长度

    - 参数:
        - 无

    - 返回:
        - `double`: 当前音频的长度，以毫秒为单位

* `GetTrackProgress`
    - 获取当前音频的进度

    - 参数:
        - 无

    - 返回:
        - `double`: 当前音频的进度，以毫秒为单位

## io.matrix_feather.dbus.CurrentBeatmap

### 描述
用于获取当前谱面信息

### 函数、参数、以及返回值

* `GetBPM`
    - 获取当前谱面BPM

    - 参数:
        - 无

    - 返回:
        - `double`: 当前谱面BPM

* `GetCurrentDifficultyStar`
    - 获取当前谱面难度星级

    - 参数:
        - 无

    - 返回:
        - `string`: 当前谱面难度星级

* `GetCurrentDifficultyVersion`
    - 获取当前谱面难度名

    - 参数:
        - 无

    - 返回:
        - `string`: 当前谱面难度名(如 "Insane", "xxx's Extra")

* `GetFullName`
    - 获取当前谱面完整名称

    - 参数:
        - 无

    - 返回:
        - `string`: 根据"`艺术家名 - 歌曲名称`"返回当前谱面完整名(如: `tomatoism - Ｓｏｍｅｏｎｅ Ｓｐｅｃｉａｌ`、`陈致逸 @HOYO-MiX - 真红骑士，出发！`)

    - 注:
        - 艺术家名和歌曲名将优先使用Unicode版本

* `GetOnlineId`
    - 获取当前谱面在线图号

    - 参数:
        - 无

    - 返回:
        - `int n1`: 在线图号

    - 注:
        - 前往 "`https://osu.ppy.sh/beatmaps/{n1}`" 可以直接查询到该图号对应的谱面信息

## io.matrix_feather.dbus.CurrentUser

### 描述
用于获取当前用户信息

### 函数、参数、以及返回值

* `GetCurrentRuleset`
    - 获取当前游戏模式

    - 参数:
        - 无

    - 返回:
        - `string`: 游戏模式全名

* `GetLaunchTick`
    - 获取当前游戏运行时间

    - 参数:
        - 无

    - 返回:
        - `long`: 游戏运行时间(`DateTime.Now.Ticks - loadTick.Ticks`)

* `GetPP`
    - 显而易见

    - 参数:
        - 无

    - 返回:
        - `string`: pp数

* `GetUserActivity`
    - 获取用户当前状态

    - 参数:
        - 无

    - 返回:
        - `string`: 当前状态(如: "正在选图"、"正在戳泡泡"、"闲置")

* `GetUserAvatarUrl`
    - 获取用户头像URL

    - 参数:
        - 无

    - 返回:
        - `string`: 头像URL

    - 注:
        - 若用户没有登录，则返回空字符串(string.Empty)

* `GetUserCountryRegionRank`
    - 获取用户在当前国家/地区的排名

    - 参数:
        - 无

    - 返回:
        - `string`: 排名结果

    - 注:
        - 若用户没有登录，则返回-1

* `GetUserRank`
    - 获取用户的全球排名

    - 参数:
        - 无

    - 返回:
        - `string`: 排名结果

    - 注:
        - 若用户没有登录，则返回-1

* `GetUserName`
    - 获取用户名

    - 参数:
        - 无

    - 返回:
        - `string`: 用户名

# 播放器插件DBus API

## io.matrix_feather.mvis.collection

### 描述
用于获取当前收藏夹信息

### 函数、参数、以及返回值
* `GetCurrentCollectionName`
    - 获取当前收藏夹名

    - 参数:
        - 无
    
    - 返回:
        - `string`: 当前收藏夹名

    - 注:
        - 若未选择收藏夹，或插件被禁用，则返回"`-`"
        - 用户可能也会将收藏夹名称设置为"`-`"

* `GetCurrentIndex`
    - 获取当前歌曲在收藏夹中的位置

    - 参数:
        - 无

    - 返回:
        - `int`: 当前位置

    - 注:
        - 若未选择收藏夹、插件被禁用，或当前歌曲不在收藏夹中，则返回-1

## io.matrix_feather.mvis.lyric

### 描述
用于获取当前歌词信息

### 函数、参数、以及返回值
* `GetCurrentLineRaw`
    - 获取当前原始歌词

    - 参数:
        - 无
    
    - 返回:
        - `string`: 歌词内容

    - 注:
        - 若插件被禁用，则返回"`-`"
        - 用户可能也会将原始歌词设置为"`-`"

* `GetCurrentLineTranslated`
    - 获取当前翻译歌词

    - 参数:
        - 无

    - 返回:
        - `string`: 歌词内容

    - 注:
        - 若插件被禁用，则返回"`-`"
        - 用户可能也会将翻译歌词设置为"`-`"

# 在Mvis插件中添加自己的DBus服务

比方说，要注册一个`FooDBusObject`到DBus上，你可以：
```C#
//创建新的DBus服务
private readonly FooDBusObject dbusObject = new FooDBusObject();

...

[BackgroundDependencyLoader]
private void load(...)
{
    ...

    //向插件管理器提交注册
    PluginManager.RegisterDBusObject(dbusObject);

    if (MvisScreen != null)
    {
        //在播放器退出时执行onMvisExiting
        MvisScreen.OnScreenExiting += onMvisExiting;
    }
}

...

//退出播放器时要执行的操作
private void onMvisExiting()
{
    ...

    //向插件管理器提交反注册
    PluginManager.UnRegisterDBusObject(dbusObject);

    ...
}
```