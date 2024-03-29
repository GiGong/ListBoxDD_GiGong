﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ListBoxDD_GG
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public ListBox listBox
        {
            get { return listBoxDD.ListBoxDD; }
            set { listBoxDD.ListBoxDD = value; }
        }

        public MainWindow()
        {
            InitializeComponent();

            for (int i = 1; i <= 30; i++)
            {
                listBox.Items.Add(new GiGong.GGData(i));
            }
        }
    }
}
