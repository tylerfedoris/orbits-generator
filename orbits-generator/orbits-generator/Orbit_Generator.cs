﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orbits_generator
{
    class Orbit_Generator
    {
        private int a, b, c, n; //variables for (ax^2 + bx + c) % n
        private int start_element, current_element, next_element;
        private int offset;
        private bool all_real, even, odd;
        private Orbit current_orbit;
        private List<Orbit> orbits;
        private List<Whisker> whiskers;
        private List<int> series;

        Orbit_Generator()
        {
            a = b = c = n = 0;
            start_element = 0;
            offset = 0;
            all_real = even = odd = false;
            orbits = null;
            series = null;
        }

        public Orbit_Generator(int a, int b, int c, int n, int k, bool all_real, bool even, bool odd)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.n = (int)Math.Pow(n, k);
            this.all_real = all_real;
            this.even = even;
            this.odd = odd;
            orbits = new List<Orbit>();
            whiskers = new List<Whisker>();
            series = new List<int>();

            if (all_real)
            {
                start_element = 0;
                offset = 1;
            }
            else if (odd)
            {
                start_element = 1;
                offset = 2;
            }
            else if (even)
            {
                start_element = 2;
                offset = 2;
            }

            Series_Init();
        }

        public void Generate_Orbit()
        {
            current_orbit = new Orbit();
            next_element = int.MinValue;

            start_element = series[0];
            current_element = start_element;
            current_orbit.elements.Add(current_element);

            do
            {
                next_element = (a * (int)Math.Pow(current_element, 2)) + (b * current_element) + c;
                next_element %= n;

                if (next_element == start_element)
                {
                    Close_Orbit();
                }
                else
                {
                    if (current_orbit.elements.Exists(e => e.Equals(next_element)))
                    {
                        var start_el_index = current_orbit.elements.FindIndex(e => e.Equals(start_element));
                        var next_el_index = current_orbit.elements.FindIndex(e => e.Equals(next_element));
                        current_orbit.elements.RemoveRange(start_el_index, next_el_index - start_el_index);
                        Close_Orbit();
                    }
                    else
                    {
                        bool in_orbit = false;
                        foreach (Orbit orbit in orbits)
                        {
                            if (orbit.elements.Exists(e => e.Equals(next_element)))
                            {
                                in_orbit = true;
                                orbit.whisker_count++;
                                break;
                            }
                        }

                        if (in_orbit)
                        {
                            Create_Whisker();
                        }
                        else
                        {
                            bool in_whiskers = false;

                            foreach (Whisker whisker in whiskers)
                            {
                                if (whisker.WhiskerValue == next_element)
                                {
                                    in_whiskers = true;
                                    break;
                                }
                            }

                            if (in_whiskers)
                            {
                                Create_Whisker();
                            }

                            else
                            {
                                current_orbit.elements.Add(next_element);
                                current_element = next_element;
                            }
                        }
                    }
                }
            } while (series.Count > 0);

            return;
        }

        private void Series_Init()
        {
            int i = start_element;
            while (i < n)
            {
                series.Add(i);
                i += offset;
            }
        }

        public void Print_Orbits()
        {
            foreach (var orbit in orbits)
            {
                orbit.cycles = orbit.elements.Count - 1;
            }

            orbits.Sort();

            foreach (var orbit in orbits)
            {
                Console.WriteLine("\nOrbit:");
                int orbit_size = orbit.elements.Count;
                int element_place = 0;
                foreach(var element in orbit.elements)
                {
                    element_place++;
                    Console.Write(element);
                    if (element_place < orbit_size) Console.Write(" -> ");
                    else Console.Write("\n");
                    if (element_place % 10 == 0) Console.WriteLine();
                }
                if (orbit.cycles > 1)
                    Console.WriteLine($"Orbit Size: {orbit.cycles} cycles");
                else
                    Console.WriteLine($"Orbit Size: {orbit.cycles} fixed point");

                Console.WriteLine($"Whiskers: {orbit.whisker_count}\n");
                Console.WriteLine("----------------------------------------");
            }

            if (whiskers.Count > 0) Print_Whiskers();
        }

        public void Print_Inverses()
        {
            Console.WriteLine("----------------------------------------\n");
            Console.WriteLine("Inverses:");
            for (int i = 1; i < series.Count; i++)
            {
                for (int j = 0; j < series.Count; j++)
                {
                    if ((series[i] * series[j]) % n == 1)
                    {
                        Console.WriteLine($"{series[i]} <-> {series[j]}");
                    }
                }
            }
            Console.WriteLine("\n----------------------------------------");
        }

        private void Print_Whiskers()
        {
            Console.WriteLine($"Total Whiskers : {whiskers.Count}");
            whiskers.Sort();
            var whisker_change_indicator = -1;
            foreach (Whisker whisker in whiskers)
            {
                if (whisker.ConnectedValue > whisker_change_indicator)
                {
                    whisker_change_indicator = whisker.ConnectedValue;
                    Console.WriteLine("----------------------------------------");
                }
                Console.Write(whisker.ToString());
                if (!whisker.connected_to_orbit) Console.WriteLine("*");
                else Console.WriteLine();
            }
        }

        private void Remove_From_Series(Orbit orbit)
        {
            foreach (int element in orbit.elements)
            {
                if (series.Exists(s => s.Equals(element))) series.Remove(element);
            }
        }

        public void Print_Analytics()
        {
            Console.WriteLine("\n----------------------------------------");
            Console.WriteLine("Orbit Analytics");
            Console.WriteLine("----------------------------------------");

            int current_orbit_size = orbits[0].cycles;
            int count = 0;

            foreach (var orbit in orbits)
            {
                if (orbit.cycles != current_orbit_size)
                {
                    if (current_orbit_size == 1)
                        Console.WriteLine($"Fixed Points: {count}");
                    else
                        Console.WriteLine($"{current_orbit_size}-Cycle Orbits: {count}");
                    current_orbit_size = orbit.cycles;
                    count = 1;
                }
                else count++;
            }
            Console.WriteLine($"{current_orbit_size}-Cycle Orbits: {count}");
            Console.WriteLine("----------------------------------------\n");
        }

        private void Close_Orbit()
        {
            current_orbit.elements.Add(next_element);
            orbits.Add(current_orbit);
            Remove_From_Series(current_orbit);
            if (series.Count > 0) Generate_Orbit();
        }

        private void Create_Whisker()
        {
            if (series.Exists(s => s.Equals(current_element))) series.Remove(current_element);
            Whisker new_whisker = new Whisker
            {
                WhiskerValue = current_element,
                ConnectedValue = next_element,
            };
            whiskers.Add(new_whisker);
            if (series.Count > 0) Generate_Orbit();
        }


    }
}
