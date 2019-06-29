namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
    {
        public void Info(string message)
        {
            #region 取得登入失敗次數並且寫log
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
            #endregion
        }

    }
}