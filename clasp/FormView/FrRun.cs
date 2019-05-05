using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MotionCtrl;
using System.Threading;

namespace clasp
{
    public partial class FrRun : Form
    {
        public bool bupdate;
        public FrRun()
        {
            InitializeComponent();
        }

        private void FrRun_Load(object sender, EventArgs e)
        {
            VAR.sys_inf.Init(lb_war_inf, MT.GPIO_OUT_ALM_RED, MT.GPIO_OUT_ALM_GREEN, MT.GPIO_OUT_ALM_YELLOW, MT.GPIO_OUT_ALM_BEEPER, VAR.gsys_set.beep_tmr);//lb_war_inf
            VAR.msg.StartUpdate(dgv_msg);
        }

        private void btn_run_Click(object sender, EventArgs e)
        {
            VAR.msg.AddMsg(Msg.EM_MSGTYPE.NOR, string.Format("开始按钮"));
            //检测复位状态
            //foreach (CARD card in MT.CardList)
            //{
            //    if (card.isReady == false)
            //    {
            //        MessageBox.Show(string.Format("{0}未初始化!", card.disc), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //        return;
            //    }
            //    foreach (AXIS ax in card.AxList)
            //    {
            //        if (ax.home_status != AXIS.HOME_STA.OK)
            //        {
            //            MessageBox.Show(string.Format("{0} 状态异常，{1}!\r\n请先复位", ax.disc, ax.home_status), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //            return;
            //        }
            //    }
            //}

            //检测运行状态
            if (VAR.gsys_set.status != EM_SYS_STA.RUN)
            {
                VAR.gsys_set.bquit = false;
                BaseAction.run();
            }
            //else
            //{
            //    warning fr;
            //    fr = (warning)FindForm("warning");
            //    if (fr != null)
            //    {
            //        fr.btn_ok.BeginInvoke(new Action(() => { fr.btn_ok.PerformClick(); }));
            //    }

            //}
        }

        private void btn_stop_Click(object sender, EventArgs e)
        {
            for (int n = 0; n < 10; n++)
            {
                VAR.gsys_set.bquit = true;
                //UploadModle.bquit = true;
                //DownloadModle.bquit = true;
                Thread.Sleep(10);
                Application.DoEvents();
            }
        }
    }
}
