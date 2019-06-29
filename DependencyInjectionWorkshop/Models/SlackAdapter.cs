using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public interface INotification
    {
        void PushMessage(string account);
    }

    public class Notification : INotification
    {
        public void PushMessage(string account)
        {
            #region 登入失敗發送slack通知
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(slackResponse => { }, "my channel", $"{account} message", "my bot name");
            #endregion
        }
    }
}