using DATMultiTrackPlayer.ViewModel;
using System;
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

namespace DATMultiTrackPlayer.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private MainWindowViewModel viewModel;
		public MainWindow()
		{
			InitializeComponent();
			Loaded += MainWindow_Loaded;
		}

		private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			viewModel = new MainWindowViewModel();
			this.DataContext = viewModel;
			viewModel.LoadedTrackSP = this.CurrentTracksSP;
			viewModel.AllTrackSP = this.TracksSP;
			this.Window_SizeChanged(null,null);
			await viewModel.InitAsync();
			this.FilerTB.TextChanged += viewModel.FilterChanged;
		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if(viewModel!=null)
				viewModel.StacksHeight = this.Height - 200;
		}
	}
}
