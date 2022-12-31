using System.Linq;

namespace QiLabMonitor
{
    public class QuantizedInertia
    {
        public static QuantizedInertia singleton = new QuantizedInertia();
        public Siglent siglent = new Siglent();

        public string[] programs =  {
            "PowerOff",
            "SetVolts:0.6,PowerOn",
            "ResetClock,SetVolts:0.6,PowerOn,Pause:10,StepVolts:50,Up:3500,Pause:10,Down,PowerOff",
            "SetVolts:0.6,PowerOn,Pause:10,StepVolts:50,Up:3500,Pause:10,Down,PowerOff",
        };

        public string title = "Quantized Inertia Pendulum Test";
        public string comment1 = "Test Name: Jumbo3";
        public string comment2 = "Heating test";
        public int sweep = 1;

        public double setVolts
        {
            get => siglent.setVolts;
            set
            {
                siglent.setVolts = value;
                _expiermentVolts = (int)((value * 486.0) + 54.3);
            }
        }

        private int _expiermentVolts = 100;
        public int expiermentVolts
        {
            get => _expiermentVolts;
            set
            {
                _expiermentVolts = value;
                //siglent.setVolts = (qi._expiermentVolts - 215.0) / 223.0; // Center Tap formula
                siglent.setVolts = (_expiermentVolts - 54.3) / 486.0; // Full formula
            }
        }


        public int stepVolts = 10;
        public int maxExpiermentVolts = 5000;
        public int physicalExpiermentVoltsLimit = 6000;

        public DateTime startTime = DateTime.Now;
        public bool direction = true;
        public string instruction = ""; //PU
        public int sweepPause = 0;

        public int lastLogWrite = -1;

        public void OnTick()
        {
            bool onceASecond = lastLogWrite != DateTime.Now.Second;
            RunSweep(onceASecond);
            siglent.ReadValues();
            if (onceASecond) WriteLog();
            lastLogWrite = DateTime.Now.Second;
        }

        private void RunSweep(bool onceASecond)
        {
            if (!string.IsNullOrWhiteSpace(instruction))
            {
                var instructions = instruction.Split(',').ToList();
                if (instructions.Count > 0)
                {
                    var parts = instructions[0].Split(':');
                    switch (parts[0])
                    {
                        case "ResetClock":
                            startTime = DateTime.Now;
                            instructions.Remove(instructions[0]);
                            break;
                        case "PowerOn":
                            siglent.powerEnabled = true;
                            instructions.Remove(instructions[0]);
                            break;
                        case "PowerOff":
                            siglent.powerEnabled = false;
                            instructions.Remove(instructions[0]);
                            break;
                        case "SetVolts":
                            if (parts.Length > 1 && double.TryParse(parts[1], out double _setVolts)) setVolts = _setVolts;
                            siglent.powerEnabled = true;
                            instructions.Remove(instructions[0]);
                            break;
                        case "StepVolts":
                            if (parts.Length > 1 && int.TryParse(parts[1], out int _stepVolts)) stepVolts = _stepVolts;
                            instructions.Remove(instructions[0]);
                            break;
                        case "Up":
                            if (onceASecond)
                            {
                                if (parts.Length > 1 && int.TryParse(parts[1], out int _maxEVolts)) { maxExpiermentVolts = _maxEVolts; instructions[0] = "Up"; }
                                expiermentVolts += stepVolts;
                                if (expiermentVolts >= maxExpiermentVolts) instructions.Remove(instructions[0]);
                                if (siglent.setVolts >= 14.6) instructions.Remove(instructions[0]);
                            }
                            break;
                        case "Down":
                            if (onceASecond)
                            {
                                expiermentVolts -= stepVolts;
                                if (siglent.setVolts <= .6)
                                {
                                    instructions.Remove(instructions[0]);
                                    sweep++;
                                    direction = !direction;
                                    System.Console.Beep();
                                }
                            }
                            break;
                        case "Pause":
                            if (parts.Length > 1)
                            {
                                int.TryParse(parts[1], out sweepPause);
                                instructions[0] = "Pause";
                            }
                            if (onceASecond) --sweepPause;
                            if (sweepPause <= 0) instructions.Remove(instructions[0]);
                            break;
                        default:
                            throw new Exception(parts[0] + " is not a reconized program command");
                    }
                }
                instruction = string.Join(',', instructions);
            }
        }

        private void WriteLog()
        {
            try
            {
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
            catch (Exception) { }
        }

    }
}
