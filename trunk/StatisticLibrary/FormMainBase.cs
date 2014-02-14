using System.Windows.Forms;
using System.Threading;
using System.Drawing;

namespace StatisticCommon
{
    public delegate void DelegateFunc();
    public delegate void DelegateIntFunc(int param);
    public delegate void DelegateIntIntFunc(int param1, int param2);
    public delegate void DelegateStringFunc(string param);
    public delegate void DelegateBoolFunc(bool param);
    public delegate void DelegateObjectFunc(object obj);
    public delegate void DelegateRefObjectFunc(ref object obj);

    public abstract class FormMainBase : Form
    {
        protected FormWait formWait;

        protected object lockEvent;
        private object lockValue;
        private int waitCounter;

        private Thread m_threadFormWait;

        protected DelegateFunc delegateStartWait;
        protected DelegateFunc delegateStopWait;
        protected DelegateFunc delegateStopWaitForm;
        protected DelegateFunc delegateEvent;
        protected DelegateFunc delegateUpdateActiveGui;
        protected DelegateFunc delegateHideGraphicsSettings;
        protected DelegateFunc delegateParamsApply;

        protected bool show_error_alert = false;

        protected FormMainBase()
        {
            InitializeComponent();

            formWait = new FormWait();
            delegateStopWaitForm = new DelegateFunc(formWait.StopWaitForm);

            delegateStartWait = new DelegateFunc(StartWait);
            delegateStopWait = new DelegateFunc(StopWait);
        }

        private void InitializeComponent()
        {
            lockEvent = new object();

            lockValue = new object();
            waitCounter = 0;
        }

        public static void ThreadProc(object data)
        {
            FormWait fw = (FormWait)data;
            fw.StartWaitForm();
        }

        public void StartWait()
        {
            lock (lockValue)
            {
                if (waitCounter == 0)
                {
                    //this.Opacity = 0.75;
                    if (m_threadFormWait != null && m_threadFormWait.IsAlive)
                        m_threadFormWait.Join();
                    else
                        ;

                    m_threadFormWait = new Thread(new ParameterizedThreadStart(ThreadProc));
                    formWait.Location = new Point(this.Location.X + (this.Width - formWait.Width) / 2, this.Location.Y + (this.Height - formWait.Height) / 2);
                    m_threadFormWait.IsBackground = true;
                    m_threadFormWait.Start(formWait);
                }
                else
                    ;

                waitCounter++;
            }
        }

        public void StopWait()
        {
            lock (lockValue)
            {
                waitCounter--;
                if (waitCounter < 0)
                    waitCounter = 0;

                if (waitCounter == 0)
                {
                    //this.Opacity = 1.0;
                    while (!formWait.IsHandleCreated) ;
                    formWait.Invoke(delegateStopWaitForm);
                }
                else
                    ;
            }
        }
    }
}