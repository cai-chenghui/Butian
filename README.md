# 补天 - Butian
>张三不给力，我们来补天

鬼谷八荒 Mod


## 与 Hylas 的关系
本来是在 [Hylas](https://github.com/lolligun/ModBahuang/tree/master/Hylas) 上修改的，但残月经常性失联，上次游戏更新才 PR 了一次，加上我要做更多功能在 Hylas 有些限制，又不好改 Hylas 的基础架构，干脆另开项目

## 使用说明

### 初次使用
1. 删除 `鬼谷八荒\Mods` 目录下的 `Hylas.dll`（不要删除 `Hylas` 文件夹）
2. 将 `YamlDotNet.dll` 放入 `鬼谷八荒\MelonLoader\Managed` 目录
3. 将 `Butian.dll` 放入 `鬼谷八荒\Mods` 目录
4. 启动鬼谷八荒（初次运行自动从 `Hylas` 导入）
5. 导入的图片若显示不全，修改资源配置文件的 `newSprite: true` 就好了

### 更新
1. 将 `Butian.dll` 放入 `鬼谷八荒\Mods` 目录
2. 启动鬼谷八荒（初次运行自动从 `Hylas` 导入）

### 更多
补天会自动生成配置文件 `config.yml`，配置文件中我加了大量注释，相信你看得懂。

### BUG
目前已知的是 `Effect/UI/beijinglong1` 修改后捏人的时候鼠标位移到指定区域会花屏，所以别改他