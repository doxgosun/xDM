# 安全交付

## 如果你处理一个数据要确保处理完成（就算断电、死机、各种意外），请使用这个工具

## 它将每一个处理的数据保存到磁盘，处理完成后删除，所以如果短时间处理大量数据，它可能会对磁盘造成较大的压力，这种情况请使用队列系统如 kafka

### demo
        public bool HandleData(string data)
        {
            //模拟成功失败
            var rd = new Random();
            if (rd.Next(0, 10) > 5)
            {
                //成功
                return true;
            }
            else
            {
                return false;
            }
        }

        public void test()
        {
            var tempDir = "z:/temp"; //临时目录
            var safeDeliverer = new xDM.xSafeDelivery.SafeDeliverer(HandleData, tempDir);
            safeDeliverer.Deliver("要安全交付的数据");
        }
