using Basis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Obt;
using Process = Basis.Process;

namespace BackgroundTasks
{
    public sealed class LiveTileTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();

            // TODO: 获取数据，更新磁贴逻辑
            await GetLatestData();
            deferral.Complete();
        }

        private IAsyncOperation<string> GetLatestData()
        {
            //try
            //{
                return AsyncInfo.Run(token => GetData());
            //}
            //catch (Exception)
            //{
            //    // ignored
            //}
            //return null;
        }


        private async Task<string> GetData()
        {
            string[] jsons = new string[Obt.DataSet.Categories.BasicArray.Length];
            ParallelOptions Opt = new ParallelOptions()
            {
                MaxDegreeOfParallelism = 6,
            };
            Parallel.For(0, Obt.DataSet.Categories.BasicArray.Length, Opt, (i, parallelLoopState) =>
            {
                string json;
                try
                {
                    int ReqCode = (1 << i) & DataSet.Categories.BasicReqCode;
                    if (ReqCode == 0) { parallelLoopState.Break(); }
                    json = Obt.DataSet.Get(ReqCode | (4401 << 16));
                }
                catch { json = "[]"; }
                if (string.IsNullOrEmpty(json)) json = "[]";
                jsons[i] = json;
            });
            new Process().UpdateTiles(jsons);
            return null;
        }
    }
}
