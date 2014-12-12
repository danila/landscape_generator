using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication2
{
    class Heightmap
    {
        private double[,] map;
        private int size;
        private int max;
        private int height;
        private int filter_size;
        Random random = new Random(Guid.NewGuid().GetHashCode());
        

        double get_element(int x, int y)
        {
            if (x > max)
                x = max;
            else if (x < 0)
                x = 0;

            if (y > max)
                y = max;
            else if (y < 0)
                y = 0;

            return map[x, y];
        }
        public Heightmap(int _detail, int _height, int _filter_size)
        {
            filter_size = _filter_size;
            height = _height;
            size = (int)Math.Pow(2, _detail) + 1;
            max = size-1;
            map = new double[size, size];
        }

        public double[,] Generate(double roughness)
        {

            map[0, 0] = random.Next() % height;
            map[0, max] = random.Next() % height;
            map[max, 0] = random.Next() % height;
            map[max, max] = random.Next() % height;

            Divide(size, roughness);
            SmoothTerrain(filter_size, size);


            return map;
        }

        private void Divide(int size, double roughness)
        {
            int x, y;
            var half = size / 2;
            var scale = roughness * size;
            if (half < 1) return;

            for (y = half; y < max; y += size)
            {
                for (x = half; x < max; x += size)
                {
                    Square(x, y, half, random.NextDouble() * scale * 2 - scale);
                }
            }

            for (y = 0; y < max; y += half)
            {
                for (x = (y + half) % size; x < max; x += size)
                {
                    Diamond(x, y, half, random.NextDouble() * scale * 2 - scale);
                }
            }

            Divide(size / 2, roughness);
        }

        private double Average(double ul, double ur, double ll, double lr)
        {
            double average = ul + ur + ll + lr;
            average /= 4;
            return average;
        }

        private void Square(int x, int y, int size, double offset)
        {
            double ave = Average(get_element(x - size, y - size),
                                 get_element(x + size, y - size),
                                 get_element(x + size, y + size),
                                 get_element(x - size, y + size));
            map[x, y] = ave + offset;
        }

        private void Diamond(int x, int y, int size, double offset)
        {
            double ave = Average(get_element(x, y - size),
                                 get_element(x + size, y),
                                 get_element(x, y + size),
                                 get_element(x - size, y));
            map[x, y] = ave + offset;
        }

        private void SmoothTerrain(int filtersize, int size)
        {
            if (filtersize == -1)
                return;

            filtersize = (int)Math.Pow(2, filtersize) + 1;
            int count = 0;
            double total = 0;

            //loop through all the values
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {

                    //count keeps track of how many points contribute to the total value at this point
                    //total stores the summation of points around this point
                    //Reset the count
                    count = 0;
                    total = 0;

                    //loop through the points around this one (contained in the filter)
                    for (int x0 = x - filtersize; x0 <= x + filtersize; x0++)
                    {
                        //if the point is outside the data set, ignore it
                        if (x0 < 0 || x0 > size - 1)
                            continue;
                        for (int y0 = y - filtersize; y0 <= y + filtersize; y0++)
                        {
                            if (y0 < 0 || y0 > size - 1)
                                continue;

                            //add the contribution from the filter to the total for this point
                            total += map[x0, y0];
                            count++;

                        } //y0
                    } //x0

                    //Store the averaged value
                    if (count != 0 && total != 0)
                        map[x,y] = total / (float)count;



                } //y
            } //x
        }
    }
}
