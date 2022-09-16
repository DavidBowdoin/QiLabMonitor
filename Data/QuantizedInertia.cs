namespace QiLabMonitor
{
    public class QuantizedInertia
    {
        public static QuantizedInertia singleton = new QuantizedInertia();
        public Siglent siglent = new Siglent();

        public string title = "Quantized Inertia Pendulum Test";
        public string comment1 = "Test Name: Jumbo3";
        public string comment2 = "";
        public int sweep = 1;

        public int expiermentVolts = 100;
        public int maxExpiermentVolts = 5000;
        public int physicalExpiermentVoltsLimit = 6000;

        public DateTime startTime = DateTime.Now;
        public bool direction = true;
        public string instruction = ""; //PU
        public int sweepPause = 100;

        public int lastLogWrite = -1;

        public void OnTick()
        {
            CalculateAndSetVoltage();
            RunSweep();
            WriteLog();
        }

        private void CalculateAndSetVoltage()
        {
            //siglent.SetVoltage((qi.expiermentVolts - 215.0) / 223.0); // Center Tap formula
            siglent.SetVoltage((expiermentVolts - 54.3) / 486.0); // Full formula
        }

      private  void RunSweep()
        {
            if (instruction == "U")
            {
                expiermentVolts += 25;
                CalculateAndSetVoltage();
                if (expiermentVolts >= maxExpiermentVolts) { instruction = "PD"; sweepPause = 50; } //3.7
                if (siglent.setVolts >= 14.6) { instruction = "PD"; sweepPause = 50; } //3.7
            }
            if (instruction.StartsWith("P"))
            {
                --sweepPause;
                if (sweepPause <= 0) { instruction = instruction.TrimStart('P'); }
            }
            if (instruction == "D")
            {
                expiermentVolts -= 10;
                CalculateAndSetVoltage();
                if (siglent.setVolts <= .6)
                {
                    instruction = "";
                    siglent.SetOutput(false);
                    sweep++;
                    direction = !direction;
                }
            }
        }

        private void WriteLog()
        {
            if(lastLogWrite != DateTime.Now.Second)
            {
                lastLogWrite = DateTime.Now.Second;
                string path = @"log.txt";

                if (!File.Exists(path))
                {
                    using (StreamWriter sw = File.CreateText(path))
                    {
                        sw.WriteLine("LogFile");
                    }
                }

                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.Write(DateTime.Now.ToString());
                    sw.Write(",");
                    sw.Write(sweep);
                    sw.Write(",");
                    sw.Write(expiermentVolts);
                    sw.Write(",");
                    sw.Write(siglent.mesuredVolts);
                    sw.Write(",");
                    sw.Write(siglent.mesuredCurrent);
                    sw.Write(","); 
                    sw.Write(siglent.mesuredPower);
                    sw.WriteLine(",");
                }

            }
        }

    }
}
