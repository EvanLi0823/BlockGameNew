namespace BlockPuzzle.NativeBridge.Enums
{
    /// <summary>
    /// 原生桥接消息类型枚举
    /// 定义Unity与原生平台之间的通信接口类型
    /// </summary>
    public enum BridgeMessageType
    {
        /// <summary>
        /// 空类型
        /// </summary>
        None = 0,

        /// <summary>
        /// 获取公共参数（语言、国家、编号）
        /// </summary>
        CommonParam = 1,

        /// <summary>
        /// 隐私政策
        /// </summary>
        PrivacyPolicy = 2,

        /// <summary>
        /// 使用条款
        /// </summary>
        TermsOfUse = 3,

        /// <summary>
        /// 播放合并音频
        /// </summary>
        PlayMergeAudio = 4,

        /// <summary>
        /// 显示提现界面
        /// </summary>
        ShowWithdraw = 5,

        /// <summary>
        /// 获取合并奖励
        /// </summary>
        GetMergeReward = 6,

        /// <summary>
        /// 埋点统计
        /// </summary>
        BuryPoint = 7,

        /// <summary>
        /// 显示视频广告
        /// </summary>
        ShowVideo = 8,

        /// <summary>
        /// 请求是否白包
        /// </summary>
        RequestIsWhiteBao = 9,

        /// <summary>
        /// 获取统一货币符号
        /// </summary>
        GetUnifyCurrency = 10,

        /// <summary>
        /// 用户反馈
        /// </summary>
        FeedBack = 11,

        /// <summary>
        /// 显示促销活动
        /// </summary>
        ShowPromotion = 12,

        /// <summary>
        /// 进入游戏
        /// </summary>
        EnterGame = 13,

        /// <summary>
        /// 更新等级
        /// </summary>
        UpdateLevel = 14,

        /// <summary>
        /// 用户金额
        /// </summary>
        UserAmount = 15,

        /// <summary>
        /// 插屏广告是否就绪
        /// </summary>
        IsInterADReady = 16,

        /// <summary>
        /// 激励视频广告是否就绪
        /// </summary>
        IsRewardADReady = 17,

        /// <summary>
        /// AdMob广告是否就绪
        /// </summary>
        IsAdMobADReady = 18,

        /// <summary>
        /// 显示提现引导
        /// </summary>
        ShowWithdrawGuide = 19,

        /// <summary>
        /// 是否有提现奖励
        /// </summary>
        IsWithdrawReward = 20
    }

    /// <summary>
    /// 广告类型枚举
    /// </summary>
    public enum AdType
    {
        /// <summary>
        /// 激励视频广告
        /// </summary>
        RewardVideo = 0,

        /// <summary>
        /// 插屏广告
        /// </summary>
        Interstitial = 1,

        /// <summary>
        /// AdMob广告
        /// </summary>
        AdMob = 2
    }

    /// <summary>
    /// 屏幕方向枚举
    /// </summary>
    public enum ScreenOrientation
    {
        /// <summary>
        /// 横屏
        /// </summary>
        Landscape = 0,

        /// <summary>
        /// 竖屏
        /// </summary>
        Portrait = 1
    }
}