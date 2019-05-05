using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using MotionCtrl;
using System.IO;
using System.Threading.Tasks;

namespace clasp
{
    public static class BaseAction
    {
        #region 加载对应产品参数

        public static EM_RES LoadProductCfg(string productname)
        {
            try
            {
                EM_RES ret = EM_RES.OK;
                VAR.sys_inf.Set(EM_ALM_STA.NOR_GREEN, "正在切换", -1, true);

                //加载
                //ret = COM.tg.LoadDat(productname);

                if (ret != EM_RES.OK)
                {
                    VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "切换产品加载异常！");
                    return ret;
                }
                else
                {
                    //加载视觉
                    //Acquistion.AllCameraDisconnect();
                    //ret = Visionimage.InitializeVSTools(productname);

                    VAR.gsys_set.cur_product_name = productname;
                    IniFile inf = new IniFile(Path.GetFullPath("..") + "\\syscfg\\syscfg.ini");
                    inf.WriteString("PRODUCT_SET", "CUR_PRODCUT_NAME", VAR.gsys_set.cur_product_name);

                    if (ret != EM_RES.OK)
                    {
                        MessageBox.Show(VAR.gsys_set.cur_product_name + "加载视觉配置失败!");
                        VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "切换产品时，视觉加载失败！当前产品为 " + VAR.gsys_set.cur_product_name);
                        return EM_RES.ERR;
                    }
                    else
                    {
                        VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "切换产品成功，当前产品为 " + VAR.gsys_set.cur_product_name);
                        return EM_RES.OK;
                    }
                }

            }
            finally
            {
                ////运行配置
                //ProductData.LoadRunCfg(VAR.gsys_set.cur_product_name);

                ////加载产品
                //int ret = COM.tg.LoadDat(VAR.gsys_set.cur_product_name);
                //if (ret != CONST.RES_OK) VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "产品数据加载失败!");
                //else VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "产品数据加载成功!");
                ////机械手
                //ret = FDH.Init(VAR.gsys_set.cur_product_name);
                //if (ret != CONST.RES_OK) VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "机械手初始化失败!");
                //else VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "机械手初始化成功!");
                ////冲模
                //ret = TD.LoadCfg(VAR.gsys_set.cur_product_name);
                //if (ret != CONST.RES_OK) VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "冲模初始化失败!");
                //else VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "冲模初始化成功!");

                VAR.sys_inf.Set(EM_ALM_STA.NOR_GREEN, "切换完成", -1, true);
            }
        }

        #endregion

        public static bool QR_Direct = false;
        public static bool DistanceCheck = false;
        public static bool bnotfeed = false;

        #region 运行

        static Task run_task = null;

        public static void run()
        {
            if (run_task == null || run_task != null && run_task.IsCompleted)
            {
                VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "创建运行线程");
                if (run_task != null) run_task.Dispose();
                run_task = new Task(run_th);
                run_task.Start();
            }
        }

        //public static void run_th_old()
        //{
        //    EM_RES res = EM_RES.OK;
        //    EM_RES res2 = EM_RES.OK;
        //    int tick = 0;

        //    VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "运行线程启动");
        //    VAR.gsys_set.status = EM_SYS_STA.RUN;
        //    VAR.sys_inf.Set(EM_ALM_STA.NOR_BLUE, "正在运行", -1);
        //    MT.GPIO_OUT_KL_START.SetOn();
        //    MT.SetAllAxToWorkSpd();

        //    //确认测试软件联机
        //    List<TestPC.InfoData> list_info = new List<TestPC.InfoData>();
        //    Turntable.tmr = System.Environment.TickCount;
        //    foreach (WS ws in COM.list_ws)
        //    {
        //        int tryCnt = 0;
        //        do
        //        {
        //            res = ws.GetTestInfo(ref list_info);
        //            if (res == EM_RES.OK) break;
        //            tryCnt++;
        //            if (tryCnt >= 3)
        //            {
        //                VAR.msg.AddMsg(Msg.EM_MSGTYPE.WAR, string.Format("{0}与测试软件通信异常！", ws.disc));
        //                VAR.gsys_set.status = EM_SYS_STA.STANDBY;
        //                VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, string.Format("{0}通信", ws.disc), 20, true);
        //                return;
        //            }
        //            VAR.msg.AddMsg(Msg.EM_MSGTYPE.WAR, string.Format("{0}与测试软件通信重连接...{1}", ws.disc, tryCnt));
        //            Thread.Sleep(1000);
        //        } while (true);
        //    }

        //    while (VAR.gsys_set.bclose == false && VAR.gsys_set.bquit == false)
        //    {
        //        tick = System.Environment.TickCount;
        //        VAR.sys_inf.Set(EM_ALM_STA.NOR_BLUE, "运行", 0, true);
        //        VAR.gsys_set.status = EM_SYS_STA.RUN;
        //        MT.GPIO_OUT_ALM_GREEN.SetOn();
        //        MT.GPIO_OUT_KL_START.SetOn();

        //        #region 安全保护

        //        //if (!MT.isSafeSen)
        //        //{
        //        //    if (bsafe == false) VAR.msg.AddMsg(Msg.EM_MSGTYPE.WAR, "安全光栅/门锁触发1");
        //        //    bsafe = true;
        //        //    if (VAR.gsys_set.status == EM_SYS_STA.RUN)
        //        //    {
        //        //        VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "安全防护");
        //        //        VAR.gsys_set.bpause = true;
        //        //    }
        //        //}
        //        //else bsafe = false;

        //        #endregion

        //        //运行测试
        //        COM.ws1.RunTestTask();
        //        COM.ws2.RunTestTask();
        //        COM.ws3.RunTestTask();
        //        COM.ws4.RunTestTask();

        //        //上下料工位准备
        //        WS workstation = Turntable.GetWSOnFeedPos;
        //        if (workstation == null)
        //        {
        //            VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "上下料前，转台位置异常!");
        //            VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "转台位置异常!", 20, true);
        //            break;
        //        }
        //        //确认是否要上下料
        //        if (!bnotfeed && (workstation.TestStatus == WS.EM_TEST_STA.COMPLETED || workstation.TestStatus == WS.EM_TEST_STA.EMPTY))
        //        {
        //            res = workstation.SetupForFeed(ref VAR.gsys_set.bquit);
        //            if (res != EM_RES.OK) break;

        //            //运行上下料线程
        //            DownloadModle.RunTask();
        //            Thread.Sleep(200);
        //            UploadModle.RunTask();
        //        }

        //        Thread.Sleep(200);

        //        //等待测试完成
        //        COM.ws1.WaitTestTask(ref VAR.gsys_set.bquit);
        //        COM.ws2.WaitTestTask(ref VAR.gsys_set.bquit);
        //        COM.ws3.WaitTestTask(ref VAR.gsys_set.bquit);
        //        COM.ws4.WaitTestTask(ref VAR.gsys_set.bquit);

        //        //测试结果
        //        foreach (WS ws in COM.list_ws)
        //        {
        //            if (ws.TestStatus == WS.EM_TEST_STA.ERROR)
        //            {
        //                VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, string.Format("{0} 测试出错!", ws.disc));
        //                VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, string.Format("{0}出错", ws.disc), 20, true);
        //                VAR.gsys_set.bpause = true;
        //                res = EM_RES.ERR;
        //                break;
        //            }
        //        }

        //        if (res != EM_RES.OK)
        //        {
        //            break;
        //        }

        //        //上下料结果
        //        if (!bnotfeed)
        //        {
        //            res = DownloadModle.WaitTask(ref VAR.gsys_set.bquit);
        //            res2 = UploadModle.WaitTask(ref VAR.gsys_set.bquit);
        //            if (res != EM_RES.OK)
        //            {
        //                VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "下料异常!");
        //                VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "下料异常!", 20, true);
        //                break;
        //            }

        //            if (res2 != EM_RES.OK)
        //            {
        //                VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "上料异常!");
        //                VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "上料异常!", 20, true);
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            //上下料工位归位
        //            workstation = Turntable.GetWSOnFeedPos;
        //            if (workstation == null)
        //            {
        //                VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "上下料后，转台位置异常!");
        //                VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "转台位置异常!", 20, true);
        //                break;
        //            }

        //            //复位测试结果
        //            workstation.ResetResultOfMd();
        //            //更新物料状态
        //            workstation.TestStatus = WS.EM_TEST_STA.UNTEST;

        //            //提前开图
        //            VAR.msg.AddMsg(Msg.EM_MSGTYPE.DBG, string.Format("{0} 启动测试", workstation.disc));
        //            res = workstation.StartTestFlow();
        //            if (res != EM_RES.OK)
        //            {
        //                Thread.Sleep(1500);
        //                res = workstation.StartTestFlow();
        //                if (res != EM_RES.OK)
        //                {
        //                    VAR.msg.AddMsg(Msg.EM_MSGTYPE.DBG,
        //                        string.Format("{0} StartTestFlow err", workstation.disc));
        //                    workstation.Status = WS.EM_STA.LINKERR;
        //                }
        //            }

        //            //计时开始
        //            workstation.tmr = System.Environment.TickCount;
        //        }

        //        //上下料工位归位
        //        workstation = Turntable.GetWSOnFeedPos;
        //        if (workstation == null)
        //        {
        //            VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "上下料后，转台位置异常!");
        //            VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "转台位置异常!", 20, true);
        //            break;
        //        }

        //        res = workstation.TurnToTest(ref VAR.gsys_set.bquit);
        //        if (res != EM_RES.OK) break;

        //        //旋转转台
        //        res = Turntable.MoveToNext(ref VAR.gsys_set.bquit);
        //        if (res != EM_RES.OK) break;

        //        //计时
        //        VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "U T=" + (Environment.TickCount - tick).ToString());
        //    }

        //    MT.GPIO_OUT_ALM_GREEN.SetOff();
        //    MT.GPIO_OUT_KL_START.SetOff();
        //    VAR.gsys_set.status = EM_SYS_STA.STANDBY;
        //    VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "运行线程结束");
        //}

        public static void run_th()
        {
            EM_RES res = MT.SetAllAxToWorkSpd();
            if (res != EM_RES.OK) return;
            VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "运行线程启动");
            VAR.sys_inf.Set(EM_ALM_STA.NOR_GREEN, "运行", 0, true);
            MT.GPIO_OUT_ALM_GREEN.SetOn();
            MT.GPIO_OUT_ALM_RED.SetOff();
            MT.GPIO_OUT_ALM_YELLOW.SetOff();
            WsTest.RunTask();


            VAR.sys_inf.Set(EM_ALM_STA.NOR_GREEN, "就绪", 0);
            MT.GPIO_OUT_ALM_GREEN.SetOff();
            MT.GPIO_OUT_KL_START.SetOff();
            VAR.gsys_set.status = EM_SYS_STA.STANDBY;
            VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "运行线程结束");
            //确认是否是点检模式

            //确认测试软件联机
            //List<TestPC.InfoData> list_info = new List<TestPC.InfoData>();
            //Turntable.tmr = System.Environment.TickCount;
            //foreach (WS ws in COM.list_ws)
            //{
            //    int tryCnt = 0;
            //    do
            //    {
            //        res = ws.GetTestInfo(ref list_info);
            //        if (res == EM_RES.OK) break;
            //        tryCnt++;
            //        if (tryCnt >= 3)
            //        {
            //            VAR.msg.AddMsg(Msg.EM_MSGTYPE.WAR, string.Format("{0}与测试软件通信异常！", ws.disc));
            //            VAR.gsys_set.status = EM_SYS_STA.STANDBY;
            //            VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, string.Format("{0}通信", ws.disc), 20, true);
            //            return;
            //        }
            //        VAR.msg.AddMsg(Msg.EM_MSGTYPE.WAR, string.Format("{0}与测试软件通信重连接...{1}", ws.disc, tryCnt));
            //        Thread.Sleep(1000);
            //    } while (true);
            //}
            ////如有测试结果错误,把错误结果清掉
            //foreach (WS ws in COM.list_ws)
            //{
            //    if (ws.Status == WS.EM_STA.ERR)
            //        ws.Status = WS.EM_STA.REDAY;
            //    if (ws.TestStatus == WS.EM_TEST_STA.ERROR)
            //        ws.TestStatus = WS.EM_TEST_STA.UNTEST;

            //}

            ////归位
            //res = MT.Move(ref VAR.gsys_set.bquit, ref MT.AXIS_UL_Z, 0, ref MT.AXIS_DL_Z, 0);
            //if (res != EM_RES.OK) return;
            //res = MT.Move(ref VAR.gsys_set.bquit, ref MT.AXIS_UL_Y, 0, ref MT.AXIS_UL_X, 0, ref MT.AXIS_DL_Y, 0);
            //if (res != EM_RES.OK) return;

            //启动线程
            //Task TskTest = new Task(() => { test_th(); });
            //TskTest.Start();
            //DownloadModle.RunTask();
            //UploadModle.RunTask();
            //VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "运行线程启动");
            //VAR.sys_inf.Set(EM_ALM_STA.NOR_GREEN, "运行", 0, true);
            //MT.GPIO_OUT_ALM_GREEN.SetOn();
            //MT.GPIO_OUT_ALM_RED.SetOff();
            //MT.GPIO_OUT_ALM_YELLOW.SetOff();
            //while (true)
            //{
            //    Thread.Sleep(10);
            //测试线程退出则通知上料/下料退出
            //if (brun == false)
            //{
            //    if (DownloadModle.status == DownloadModle.EM_STA.WAIT)
            //        DownloadModle.bquit = true;
            //    if (UploadModle.status == UploadModle.EM_STA.WAIT)
            //        UploadModle.bquit = true;
            //}
            //if (DownloadModle.brun || UploadModle.brun || brun)
            //{
            //    // VAR.sys_inf.Set(EM_ALM_STA.NOR_BLUE, "运行", 0, false);
            //    VAR.gsys_set.status = EM_SYS_STA.RUN;


            //    MT.GPIO_OUT_KL_START.SetOn();
            //    MT.GPIO_OUT_KL_RESET.SetOff();
            //    MT.GPIO_OUT_KL_STOP.SetOff();
            //}
            //else
            //{
            //    if (COM.ws1.TestStatus == WS.EM_TEST_STA.EMPTY && COM.ws2.TestStatus == WS.EM_TEST_STA.EMPTY &&
            //      COM.ws3.TestStatus == WS.EM_TEST_STA.EMPTY && COM.ws4.TestStatus == WS.EM_TEST_STA.EMPTY && VAR.ClearMt)
            //    {
            //        VAR.ClearMt = false;
            //        ////进仓储
            //        Task tsk_fd = new Task(() =>
            //        {
            //            EM_RES res_fd = EM_RES.OK;
            //            bool bq_fd = false;
            //            res_fd = UploadModle.traybox_fd.Tray_Action(TrayBox.EM_DIR.ONLY_IN);
            //            if (res_fd == EM_RES.OK)
            //                MT.Move(ref bq_fd, ref UploadModle.traybox_fd.ax_z, 0);
            //        });
            //        tsk_fd.Start();
            //        Task tsk_ok = new Task(() =>
            //        {
            //            EM_RES res_ok = EM_RES.OK;
            //            bool bq_ok = false;
            //            res_ok = DownloadModle.traybox_ok.Tray_Action(TrayBox.EM_DIR.ONLY_IN);
            //            if (res_ok == EM_RES.OK)
            //                MT.Move(ref bq_ok, ref DownloadModle.traybox_ok.ax_z, 0);
            //        });
            //        tsk_ok.Start();
            //        Task tsk_ng = new Task(() =>
            //        {
            //            EM_RES res_ng = EM_RES.OK;
            //            bool bq_ng = false;
            //            res_ng = DownloadModle.traybox_ng.Tray_Action(TrayBox.EM_DIR.ONLY_IN);
            //            if (res_ng == EM_RES.OK)
            //                MT.Move(ref bq_ng, ref DownloadModle.traybox_ng.ax_z, 0);
            //        });
            //        tsk_ng.Start();
            //    }
            //    break;
            //}

            //#region 安全保护

            //if (!MT.isSafeSen)
            //{
            //    if (bsafe == false) VAR.msg.AddMsg(Msg.EM_MSGTYPE.WAR, "安全光栅/门锁触发1");
            //    bsafe = true;
            //    if (VAR.gsys_set.status == EM_SYS_STA.RUN)
            //    {
            //        VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "安全防护");
            //        VAR.gsys_set.bpause = true;
            //    }
            //}
            //else bsafe = false;

            //#endregion
            //}
            ////归位
            //res = MT.Move(ref VAR.gsys_set.bquit, ref MT.AXIS_UL_Z, 0, ref MT.AXIS_DL_Z, 0);
            //if (res == EM_RES.OK)
            //{
            //    MT.Move(ref VAR.gsys_set.bquit, ref MT.AXIS_UL_Y, 0, ref MT.AXIS_UL_X, 0, ref MT.AXIS_DL_Y, 0);
            //}

            //VAR.sys_inf.Set(EM_ALM_STA.NOR_GREEN, "就绪", 0);
            //MT.GPIO_OUT_ALM_GREEN.SetOff();
            //MT.GPIO_OUT_KL_START.SetOff();
            //VAR.gsys_set.status = EM_SYS_STA.STANDBY;
            //VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "运行线程结束");
        }

        #endregion

        #region 测试线程
        public static bool brun = false;
        public static void test_th()
        {
            EM_RES res = EM_RES.OK;
            VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "测试线程启动");
            int tick = System.Environment.TickCount;
            try
            {
                while (VAR.gsys_set.bclose == false && VAR.gsys_set.bquit == false)
                {
                    brun = true;
                    VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "启动测试");
                    //运行测试
                    //COM.ws1.RunTestTask();
                    //COM.ws2.RunTestTask();
                    //COM.ws3.RunTestTask();
                    //COM.ws4.RunTestTask();

                    //上下料
                    //WS workstation = Turntable.GetWSOnFeedPos;
                    //if (workstation == null)
                    //{
                    //    VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "上下料前，转台位置异常!");
                    //    VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "转台位置异常!", 20, true);
                    //    break;
                    //}

                    //等待测试完成
                 
                    //if (VAR.gsys_set.isChkMode && !bfeed)
                    //    workstation.TestStatus = WS.EM_TEST_STA.EMPTY;

                    //检查是否需要下料
                   
                 

                    //等待测试完成
                    //COM.ws1.WaitTestTask(ref VAR.gsys_set.bquit);
                    //COM.ws2.WaitTestTask(ref VAR.gsys_set.bquit);
                    //COM.ws3.WaitTestTask(ref VAR.gsys_set.bquit);
                    //COM.ws4.WaitTestTask(ref VAR.gsys_set.bquit);

                    //测试结果
                    //foreach (WS ws in COM.list_ws)
                    //{
                    //    if (ws.TestStatus == WS.EM_TEST_STA.ERROR)
                    //    {
                    //        VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, string.Format("{0} 测试出错!", ws.disc));
                    //        VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, string.Format("{0}出错", ws.disc), 20, true);
                    //        VAR.gsys_set.bpause = true;
                    //        res = EM_RES.ERR;
                    //        break;
                    //    }
                    //}
                    //if (res != EM_RES.OK || VAR.gsys_set.bquit)
                    //{
                    //    VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "测试异常，测试线程结束");
                    //    break;
                    //}

                    //VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, string.Format("测试完成,T={0:F2}s", (Environment.TickCount - tick) / 1000.0));
                    //if (COM.ws1.TestStatus == WS.EM_TEST_STA.EMPTY && COM.ws2.TestStatus == WS.EM_TEST_STA.EMPTY &&
                    //    COM.ws3.TestStatus == WS.EM_TEST_STA.EMPTY && COM.ws4.TestStatus == WS.EM_TEST_STA.EMPTY && VAR.ClearMt)
                    //{
                    //    //VAR.ClearMt = false;
                    //    VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "清料完成,系统退出!");
                    //    //进仓储
                    //    //Task tsk_fd = new Task(() =>
                    //    //{
                    //    //    EM_RES res_fd=EM_RES.OK;
                    //    //    bool bq_fd=false;
                    //    //    res_fd=UploadModle.traybox_fd.Tray_Action(TrayBox.EM_DIR.ONLY_IN);
                    //    //    if (res_fd == EM_RES.OK)
                    //    //        MT.Move(ref bq_fd, ref UploadModle.traybox_fd.ax_z, 0);
                    //    //});
                    //    //Task tsk_ok= new Task(() =>
                    //    //{
                    //    //    EM_RES res_ok = EM_RES.OK;
                    //    //    bool bq_ok = false;
                    //    //    res_ok = DownloadModle.traybox_ok.Tray_Action(TrayBox.EM_DIR.ONLY_IN);
                    //    //    if (res_ok == EM_RES.OK)
                    //    //        MT.Move(ref bq_ok, ref DownloadModle.traybox_ok.ax_z, 0);
                    //    //});
                    //    //Task tsk_ng = new Task(() =>
                    //    //{
                    //    //    EM_RES res_ng = EM_RES.OK;
                    //    //    bool bq_ng = false;
                    //    //    res_ng = DownloadModle.traybox_ng.Tray_Action(TrayBox.EM_DIR.ONLY_IN);
                    //    //    if (res_ng == EM_RES.OK)
                    //    //        MT.Move(ref bq_ng, ref DownloadModle.traybox_ng.ax_z, 0);
                    //    //});
                    //    break;
                    //}



                    //旋转转台
                 
                    //旋转状态
                    //VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, string.Format("旋转状态异常, 工站:{0},下料:{1},上料:{2}", workstation.Status.ToString(), DownloadModle.status.ToString(), UploadModle.status.ToString()));
                    break;
                }
            }
            finally
            {
                brun = false;
                VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "测试线程结束");
            }
        }
        #endregion

        #region 上下料线程

        public static void UpDownLoad_th()
        {
            EM_RES res = EM_RES.OK;
            EM_RES res2 = EM_RES.OK;
            int tick = 0;

            VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "运行线程启动");
            VAR.gsys_set.status = EM_SYS_STA.RUN;
            VAR.sys_inf.Set(EM_ALM_STA.NOR_BLUE, "正在运行", -1);
            MT.GPIO_OUT_KL_START.SetOn();
            MT.SetAllAxToWorkSpd();

            while (VAR.gsys_set.bclose == false && VAR.gsys_set.bquit == false)
            {

                //上下料工位准备
                //WS workstation = Turntable.GetWSOnFeedPos;
                //if (workstation == null)
                //{
                //    VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "上下料前，转台位置异常!");
                //    VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "转台位置异常!", 20, true);
                //    break;
                //}
                //确认是否要上下料
                //if (!bnotfeed && (workstation.TestStatus == WS.EM_TEST_STA.COMPLETED || workstation.TestStatus == WS.EM_TEST_STA.EMPTY))
                //{
                //    res = workstation.SetupForFeed(ref VAR.gsys_set.bquit);
                //    if (res != EM_RES.OK) break;
                //}

                //运行上下料线程
                //DownloadModle.RunTask();
                //Thread.Sleep(200);
                //UploadModle.RunTask();




                //上下料结果
                //if (!bnotfeed)
                //{
                //    res = DownloadModle.WaitTask(ref VAR.gsys_set.bquit);
                //    res2 = UploadModle.WaitTask(ref VAR.gsys_set.bquit);
                //    if (res != EM_RES.OK)
                //    {
                //        VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "下料异常!");
                //        VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "下料异常!", 20, true);
                //        break;
                //    }

                //    if (res2 != EM_RES.OK)
                //    {
                //        VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "上料异常!");
                //        VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "上料异常!", 20, true);
                //        break;
                //    }
                //}
                //else
                //{
                //    //上下料工位归位
                //    workstation = Turntable.GetWSOnFeedPos;
                //    if (workstation == null)
                //    {
                //        VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "上下料后，转台位置异常!");
                //        VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "转台位置异常!", 20, true);
                //        break;
                //    }

                //    //复位测试结果
                //    workstation.ResetResultOfMd();
                //    //更新物料状态
                //    workstation.TestStatus = WS.EM_TEST_STA.UNTEST;

                //    //提前开图
                //    VAR.msg.AddMsg(Msg.EM_MSGTYPE.DBG, string.Format("{0} 启动测试", workstation.disc));
                //    res = workstation.StartTestFlow();
                //    if (res != EM_RES.OK)
                //    {
                //        Thread.Sleep(1500);
                //        res = workstation.StartTestFlow();
                //        if (res != EM_RES.OK)
                //        {
                //            VAR.msg.AddMsg(Msg.EM_MSGTYPE.DBG,
                //                string.Format("{0} StartTestFlow err", workstation.disc));
                //            workstation.Status = WS.EM_STA.LINKERR;
                //        }
                //    }

                    //计时开始
                //    workstation.tmr = System.Environment.TickCount;
                //}

                //上下料工位归位
                //workstation = Turntable.GetWSOnFeedPos;
                //if (workstation == null)
                //{
                //    VAR.msg.AddMsg(Msg.EM_MSGTYPE.ERR, "上下料后，转台位置异常!");
                //    VAR.sys_inf.Set(EM_ALM_STA.WAR_YELLOW_FLASH, "转台位置异常!", 20, true);
                //    break;
                //}

                //res = workstation.TurnToTest(ref VAR.gsys_set.bquit);
                //if (res != EM_RES.OK) break;

                //旋转转台
                //res = Turntable.MoveToNext(ref VAR.gsys_set.bquit);
                //if (res != EM_RES.OK) break;

                //计时
                VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, "U T=" + (Environment.TickCount - tick).ToString());
            }

            MT.GPIO_OUT_ALM_GREEN.SetOff();
            MT.GPIO_OUT_KL_START.SetOff();
            VAR.gsys_set.status = EM_SYS_STA.STANDBY;
            VAR.msg.AddMsg(Msg.EM_MSGTYPE.SYS, "运行线程结束");
        }
        #endregion

        #region 停止

        public static void stop()
        {
            int n;

            //if (VAR.gsys_set.status == CONST.SYS_STATUS_EMG || VAR.gsys_set.status == CONST.SYS_STATUS_ERR || VAR.gsys_set.status == CONST.SYS_STATUS_UNKOWN)
            {
                MT.AllAxStop();
                //    return;
            }

            //唤醒再停止
            VAR.gsys_set.bpause = false;
            // VAR.gsys_set.mre_pause.Set();
            //quit
            for (n = 0; n < 50; n++)
            {
                VAR.gsys_set.bquit = true;
                //UploadModle.bquit = true;
                //DownloadModle.bquit = true;
                Thread.Sleep(10);
                //Application.DoEvents();
            }

            //wait stop
            //EM_RES ret = TD.Task_Stop();
            EM_RES ret = EM_RES.OK;
            if (ret == EM_RES.OK)
            {
                if (VAR.gsys_set.status != EM_SYS_STA.EMG && VAR.gsys_set.status != EM_SYS_STA.ERR &&
                    VAR.gsys_set.status != EM_SYS_STA.UNKOWN)
                    VAR.gsys_set.status = EM_SYS_STA.STANDBY;
            }
        }

        #endregion

        #region 暂停

        //public static bool pause(ref EM_SYS_STA status, ref bool bquit, bool bquit2 = false)
        //{
        //    bool bpause = false;
        //    if (VAR.gsys_set.bpause == true || MT.GPIO_IN_FR_DOOR.isOFF)
        //    {
        //        bpause = true;
        //        VAR.gsys_set.bpause = true;
        //        if (VAR.gsys_set.status != EM_SYS_STA.PAUSE) MT.Beeper(100);
        //    }

        //    while ((VAR.gsys_set.mode & EM_SYS_MODE.STEP) == EM_SYS_MODE.STEP &&
        //           VAR.gsys_set.status == EM_SYS_STA.RUN || (VAR.gsys_set.bpause == true || MT.GPIO_IN_FR_DOOR.isOFF))
        //    {
        //        if (VAR.gsys_set.bpause == true || MT.GPIO_IN_FR_DOOR.isOFF)
        //        {
        //            status = EM_SYS_STA.PAUSE;
        //            VAR.gsys_set.status = EM_SYS_STA.PAUSE;
        //        }

        //        //继续运行
        //        if (MT.GPIO_IN_KEY_START.AssertON())
        //        {
        //            VAR.sys_inf.Set(EM_ALM_STA.NOR_GREEN, "运行", 0, true);
        //            break;
        //        }

        //        //复位键退出
        //        if (MT.GPIO_IN_KEY_STOP.AssertON())
        //        {
        //            VAR.sys_inf.Set(EM_ALM_STA.NOR_GREEN, "运行", 0, true);
        //            //if (!VAR.isStepMode)
        //            bquit = true;
        //            break;
        //        }

        //        if (bquit || bquit2) break;

        //        //发生错误
        //        if (VAR.gsys_set.status == EM_SYS_STA.EMG || VAR.gsys_set.status == EM_SYS_STA.ERR ||
        //            VAR.gsys_set.status == EM_SYS_STA.UNKOWN)
        //        {
        //            bquit = true;
        //            break;
        //        }

        //        Thread.Sleep(10);
        //        Application.DoEvents();
        //    }

        //    //检查系统状态
        //    if (VAR.gsys_set.status == EM_SYS_STA.RUN || VAR.gsys_set.status == EM_SYS_STA.PAUSE)
        //    {
        //        status = EM_SYS_STA.RUN;
        //        VAR.gsys_set.status = EM_SYS_STA.RUN;
        //        VAR.sys_inf.Set(EM_ALM_STA.NOR_GREEN, "运行", 0, true);
        //    }
        //    else
        //    {
        //        //bquit = true;
        //    }

        //    return bpause;
        //}

        #endregion

        #region 退出

        public static EM_RES Close()
        {
            //stop
            stop();
            VAR.gsys_set.bclose = true;
            Thread.Sleep(100);
            //Application.DoEvents();
            //Thread.Sleep(100);
            //Application.DoEvents();
            for (int n = 0; n < 100; n++)
            {
                VAR.gsys_set.bquit = true;
                VAR.gsys_set.bpause = false;
                VAR.gsys_set.bclose = true;
                Thread.Sleep(10);
                //Application.DoEvents();
            }

            VAR.gsys_set.bquit = false;
            //MT.ResetIO();

            //close card
            MT.Close();

            //disconnet camera
            //foreach (Cam cam in COM.ListCam)
            //{
            //    cam.Dispose();
            //}

            Thread.Sleep(100);
            //Application.DoEvents();
            //Thread.Sleep(100);
            //Application.DoEvents();

            return EM_RES.OK;
        }

        #endregion
    }
    class myfunc
    {
    }
}
