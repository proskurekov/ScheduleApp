using System;

namespace ScheduleApp.Data
{
    public class DataTable
    {
        public int Jobs { get; set; } = 3;
        public int Machines { get; set; } = 3;
        public int Tasks { get; set; } = 3;
        public bool Flag { get; set; }
        public bool ZeroCell { get; set; }
        public List<List<(int, int)[]>> Jbs { get; set; }


        public List<List<(int, int)[]>> InitializeJobs()
        {
            List<List<(int, int)[]>> temp = new List<List<(int, int)[]>>();
            var Tasks = this.Tasks;
            var Machines = this.Machines;
            var Jobs = this.Jobs;

            for (int i = 0; i < Jobs; i++)
            {
                temp.Add(new List<(int, int)[]>());
                for (int j = 0; j < Tasks; j++)
                {
                    temp[i].Add(new (int, int)[this.Machines]);  // Machines
                    for (int k = 0; k < Machines; k++)
                    {
                        var tuple = Tuple.Create(0, k);
                        temp[i][j][k] = tuple.ToValueTuple();
                    }
                }
            }
            return temp;
        }

        public void FillTable(int indx)
        {
            var rand = new Random();
            var val = this.ZeroCell ? 0 : 1;

            for (int i = 0; i < this.Jbs.Count; i++)
            {
                for (int j = 0; j < this.Jbs[i].Count; j++)
                {
                    for (int k = 0; k < this.Jbs[i][j].Length; k++)
                    {
                        if (indx == 0)
                        {
                            this.Jbs[i][j][k].Item1 = rand.Next(val, 10);
                        }
                        else if (i == indx - 1)
                        {
                            this.Jbs[i][j][k].Item1 = rand.Next(val, 10);
                        }
                    }
                }
            }
        }

        public void ClearTable(int indx)
        {
            for (int i = 0; i < this.Jbs.Count; i++)
            {
                for (int j = 0; j < this.Jbs[i].Count; j++)
                {
                    for (int k = 0; k < this.Jbs[i][j].Length; k++)
                    {
                        if (indx == 0)
                        {
                            this.Jbs[i][j][k].Item1 = 0;
                        }
                        else if (i == indx - 1)
                        {
                            this.Jbs[i][j][k].Item1 = 0;
                        }
                    }
                }
            }
        }
    }
}