   M S B A M L   `   `   `  ����K  GBuild&BuildersIS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null3clr-namespace:Build_BuildersISBuild_BuildersIS  5   	   I)clr-namespace:Build_BuildersIS.ViewModelsBuild_BuildersIS.ViewModels  5   	   "    Build_BuildersIS.LoginWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   0localclr-namespace:Build_BuildersIS     5   	   8vm)clr-namespace:Build_BuildersIS.ViewModels     5   	   -   5          � Title$  Login��5	   	   $	��350��5
   	   $	��299��5   	    � WindowStartupLocation$ CenterScreen=�5   	    � WindowStyle$
 None=�5   	    � AllowsTransparency . 
������  � DataContext 5      0   *Build_BuildersIS.ViewModels.LoginViewModel  5   
   .��5      �� ���	#FF484848 �� CornerRadius$ 15��6$   
���   �$��1q�6J   $��0,0,0,0q�6^   ._�5   
   � $��0,-1,0,0q�6    � RowDefinitions 5      �� 5      $��40 �6    �� 5      $	��47* �6    �� 5      $	��15* �6    .+�5      ��    CloseButtonStyle#��[  6�   $��262,12,10,3q�6�   ��6   �� *CloseCommand+��66   ��  �� RelativeSource ��  �� AncestorType$ WindowL���5      �� 5      	 �� ImageSource$7	 1pack://application:,,,/Resources/window-close.png��6!   
 j� Stretch$
 Uniform=�6a   5       �� 5!      $��Username or e-mail:��6   $��59,86,60,117q�62   $��1��6H    J� RenderTransformOrigin$ 0.5,0.5&�6U   $��14(�6u   ���	#FFB8B8B8$��Segoe UI Black)�6�   �� 5"      -   5         ��Name$ UsernameTextBox��5"        NoSelectTextBoxStyle#��[ 6/   $��58,115,60,97q�6�   $��1��6�   $��14(�6�   
������ ���	#FFB8B8B8��6]   �� *
Username+ �� UpdateSourceTrigger$ PropertyChanged=�6�   �� 5#      $��	Password:��6   $��59,145,149,58q�6(   $��1��6?   $ 
0.338,2.56&�6L   $��14(�6o   ���	#FFB8B8B8$��Segoe UI Black)�6�   �� 5$      -   5        $ PasswordBox��5$      #��[ 6+   $��58,175,60,37q�6�   $��1��6�   $��14(�6�   
������ ���	#FFB8B8B8��6Y   �� *
Password+$ PropertyChanged=�6�   �� 5%      $��LOGIN��6   $��58,23,161,22q�6�   $ 
0.15,0.118&�6�     FlatYellowButtonStyle#��[ 6�   $��2��6   $��14(�6  $��Segoe UI Black)�6  $
��Bold"�67  ��6&   �� *LoginCommand+��6G   ��  �� $ WindowL�67  �� 5&      $��REGISTER��6   $��159,23,60,22q�6�   #��[ 6�   $��2��6�   $��14(�6�   $
��Bold"�6�   $��Segoe UI Black)�6  ��6(   �� *RegisterCommand+��6L   ��  �� $ WindowL�6  �� 5'      $��I��6   $��158,33,112,174q�6    $��60(�68   $ 0.537,0.517&�6F   $��Arial)�6j   $��2��6}   ���	#FFFFCA0E$
��Bold"�6�   �� 5(      $��S��6   $��172,33,77,174q�6    $��60(�67   $ 0.537,0.517&�6E   $��Arial)�6i   $��2��6|   ���	#FFFFCA0E$
��Bold"�6�   �� 5)      $��  Build&
Builders��6   $��78,0,104,174q�6=   $��1��6S   $��20(�6`   $
��Bold"�6n   ���	#FFFFCA0E$��Segoe UI Black)�6�   �� 5*        MinimizeButtonStyle#��[ 6�   $��237,12,35,3q�6�   ��6   �� *MinimizeCommand+��69   ��  �� $ WindowL���5+      �� 5,      $0	 *pack://application:,,,/Resources/minus.png��6!   $
 Uniform=�6Z   5.      �� 5/      -   5        $��Overlay��5/      ���	#80000000$y�	Collapsed=�6;   $ 15��6R   $��3��6d   ._�50      � 51      53      54      55      