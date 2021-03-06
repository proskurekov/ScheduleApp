using Google.OrTools.Sat;

namespace ScheduleApp.Data
{
    public class Optimize
    {
        private static List<List<(int,int)[]>> jobs;         

        public Optimize (List<List<(int,int)[]>> j)
        {
            jobs = j; 
        }

        public void Solve()
        {
            var num_jobs = jobs.Count;
            var all_jobs = Enumerable.Range(0, num_jobs);
            var num_machines = 3;
            var all_machines = Enumerable.Range(0, num_machines);
            
            var model = new CpModel();

            var horizon = 0; 
            foreach (var job in jobs)
            {
                foreach (var task in job)
                {
                    var max_duration = 0; 
                    foreach (var alt in task)
                    {
                        max_duration = Math.Max(max_duration,alt.Item1);
                    }
                    horizon += max_duration; 
                }
            }

            var intervalPerResources = new Dictionary<int,List<IntervalVar>>(){};
            //Tuple
            var starts = new Dictionary<Tuple<int,int>,IntVar>();
            var presences = new Dictionary<Tuple<int,int,int>,IntVar>(); // (job,task,alt)
            var job_ends = new List<IntVar>();

            foreach (var job_id in all_jobs)
            {
                var job = jobs[job_id]; 
                var num_tasks = job.Count; //такски из каждой работы
                object previous_end = null; 

                foreach (var task_id in Enumerable.Range(0,num_tasks)) // 0..3
                {
                    var task = job[task_id]; //возвращает правильно таски
                    var min_duration = task[0].Item1; // max 
                    var max_duration = task[0].Item1; //min
                    var num_alt = task.Length; 
                    var all_alt = Enumerable.Range(0,num_alt); //all alt

                    foreach (var alt_id in Enumerable.Range(1,num_alt-1))
                    {
                        var alt_duration = task[alt_id].Item1;
                        min_duration = Math.Min(min_duration,alt_duration);
                        max_duration = Math.Max(max_duration,alt_duration);
                    }

                    // Create main interval for the task.
                    var suffix_name = $"_j{job_id}_t{task_id}";
                    var start = model.NewIntVar(0,horizon,"start" + suffix_name);
                    var duration = model.NewIntVar(min_duration, max_duration,"duration" + suffix_name);
                    var end = model.NewIntVar(0, horizon, "end" + suffix_name);
                    var interval = model.NewIntervalVar(start, duration, end,"interval" + suffix_name);
                    var key = Tuple.Create(job_id,task_id); // ключ для словаря
                    starts[key] = start;

                    if (previous_end != null) {
                        
                        model.Add(start >= (IntVar)previous_end);
                    }
                    previous_end = end; 

                    //Альтернативные интервалы
                    
                    // Число задач > 1
                    if (num_alt > 1) {
                        var lPresences = new List<BoolVar>(){};
                        foreach (var altID in all_alt)
                        {
                            var altSuffix = $"_j{job_id}_t{task_id}_a{altID}";
                            var lPresence = model.NewBoolVar("presence" + altSuffix);
                            var lStart = model.NewIntVar(0,horizon,"start" + altSuffix);
                            var lDuration = task[altID].Item1; 
                            var lEnd = model.NewIntVar(0,horizon,"end" + altSuffix);
                            var lInterval = model.NewOptionalIntervalVar(lStart,lDuration,lEnd,lPresence,"interval" + altSuffix); // NewOptionalIntervalVar ???
                            lPresences.Add(lPresence); // add or append
                            
                            model.Add(start == lStart).OnlyEnforceIf(lPresence); 
                            model.Add(duration == lDuration).OnlyEnforceIf(lPresence);
                            model.Add(end == lEnd).OnlyEnforceIf(lPresence);

                            if (!intervalPerResources.ContainsKey(task[altID].Item2))
                            {
                                intervalPerResources.Add(task[altID].Item2, new List<IntervalVar>());
                            }

                            
                            intervalPerResources[task[altID].Item2].Add(lInterval);
            
                            var altKey = Tuple.Create(job_id,task_id,altID);
                            presences[altKey] = lPresence; 
                        } 
                        model.AddExactlyOne(lPresences);
                    }
                    else 
                    {
                        //intervalPerResources.Add(interval);
                        var noAltKey = Tuple.Create(job_id,task_id,0);
                        presences[noAltKey] = model.NewConstant(1);
                    }                    
                }
                job_ends.Add((IntVar)previous_end);
            }
            
            //ограничения для машин
            foreach (var machineID in all_machines)
            {
                var intervals = intervalPerResources[machineID];
                //model.AddNoOverlap(intervals);
                if (intervals.Count > 1)
                {
                    model.AddNoOverlap(intervals);
                }
            }

            var makespan = model.NewIntVar(0,horizon,"makespan");
            model.AddMaxEquality(makespan,job_ends);
            model.Minimize(makespan);

            //решение

            var solver = new CpSolver();
            var status = solver.Solve(model);
            var results = new List<string>();

            //System.Console.WriteLine(status);

            foreach (var job_id in all_jobs)
            {
                foreach(var task_id in Enumerable.Range(0,jobs[job_id].Count))
                {
                    var k = Tuple.Create(job_id,task_id);
                    var start_value = solver.Value(starts[k]);
                    var machine = -1; 
                    var duration = -1;
                    var selected = -1;

                    foreach (var alt_id in Enumerable.Range(0,jobs[job_id][task_id].Length))
                    {
                        //System.Console.WriteLine(alt_id);
                        //System.Console.WriteLine(solver.Value(presences[Tuple.Create(job_id,task_id,alt_id)]));
                        if (solver.Value(presences[Tuple.Create(job_id,task_id,alt_id)]) == 1)
                        {
                            duration = jobs[job_id][task_id][alt_id].Item1;
                            machine = jobs[job_id][task_id][alt_id].Item2;
                            selected = alt_id;
                        }

                        //System.Console.WriteLine($"task_{job_id}_{task_id} starts at {start_value} (alt {selected}, machine {machine}, duration {duration})");
                    }
                    // System.Console.WriteLine($"task_{job_id}_{task_id} starts at {start_value} (alt {selected}, machine {machine}, duration {duration})");
                    // results.Add($"task_{job_id}_{task_id} starts at {start_value} (alt {selected}, machine {machine}, duration {duration})");
                }
            }
            //return results; 
        } 
        
    }
}