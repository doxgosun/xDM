# xDM.xConsole

命令行下实现一个Python中CMDLoop类似的东西，功能更屌一些

## 使用方法：

    public class DConsole : xDM.xConsole.ConsoleBase
    {
        public override string Info { get; set; }
        public override IEnumerable<TextInfo> Infos
        {
            get
            {
                yield return new TextInfo($"当前时间：", ConsoleColor.Yellow);
                yield return new TextInfo($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}", ConsoleColor.Green);
            }
            set { }
        }
        protected override string ConfigFileName { get; set; } = "配置.cfg";

        private DConfig Config { get; set; }
        protected override object X_Config => Config;

        public DConsole()
        {
            Config = LoadConfig<DConfig>();
        }

        [DefaultHelpInfo("开始")]
        public void DO_Start(string args)
        {
            Task.Run(() =>
            {
                Prompt = "处理中。。。。";
                Thread.Sleep(1000);
                Prompt = BasePrompt;
                ShowSucess("完成！");
            });
        }
    }

    public class DConfig
    {
        /// <summary>
        /// 临时目录
        /// </summary>
        [IsDir("temp")]
        public string TempDir { get; set; }

        /// <summary>
        /// 什么数量
        /// </summary>
        [IsNumber(10, 20)]
        public int SomeNum { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var cmd = new DConsole();
            cmd.Prompt = "数据清洗>";
            cmd.CmdLoop();
        }
    }

