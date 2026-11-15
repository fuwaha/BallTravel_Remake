namespace BallTravel_Remake.Scripts.UI
{
    /// <summary>
    ///     PlayerUI配下に置くUI部品が実装するインターフェース。
    ///     PlayerUIが起動時に自動で見つけて Bind を呼ぶ
    /// </summary>
    public interface IPlayerUIPart
    {
        void Bind(PlayerUI playerUI);
    }
}
