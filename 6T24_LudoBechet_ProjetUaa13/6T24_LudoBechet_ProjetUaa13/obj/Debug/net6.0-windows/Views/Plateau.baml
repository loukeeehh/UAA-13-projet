   M S B A M L   `   `   `  ����V  R6T24_LudoBechet_ProjetUaa13, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullW0clr-namespace:_6T24_LudoBechet_ProjetUaa13.Views"_6T24_LudoBechet_ProjetUaa13.Views  5      0    *_6T24_LudoBechet_ProjetUaa13.Views.Plateau   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Blocal0clr-namespace:_6T24_LudoBechet_ProjetUaa13.Views     5        N� Title$  Plateau��5      .��5      � -   5        $��grdMain��5      
���   � � RowDefinitions 5	   
   �� 5
      $��1* �6   �� 5      $��3* �6   �� 5      $��1* �6    � ColumnDefinitions 5   
   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   �� 5      $��1* �6   .+�5   
   �� $��0��6   $��0��6   $��3��6-   $��7��6>   $��../Asset/fond.png��5      $
��Fill=�5      �� 5   
   -   5        $��1��5      $	��160��6   $	��680��6*   $��2��67   $��1��6H   
����� �
 � Tag$ defenseJoueur1��5   :   ._�5       �� -   5         ��Name$ StackZoneJoueur1Defense��5        �� Orientation$ Vertical=�6:   $	��Top=�5!      $��Center=�6+   5"      �� 5#   
   -   5        $��1��5#      $	��160��6   $	��680��6*   $��2��67   $��2��6H   
���  ��$ attackJoueur1��5$   9   ._�5%      �� -   5        $ StackZoneJoueur1Attack��5%      $ Vertical=�69   $	��Top=�5&      $��Center=�6+   5'      �� 5*   
   -   5        $��1��5*      $	��160��6   $	��680��6*   $��2��67   $��5��6H   
����� �$ defenseJoueur2��5+   :   ._�5,      �� -   5        $ StackZoneJoueur2Defense��5,      $ Vertical=�6:   $	��Top=�5-      $��Center=�6+   5.      �� 5/   
   -   5        $��1��5/      $	��160��6   $	��680��6*   $��2��67   $��4��6H   
���  ��$ attackJoueur2��50   9   ._�51      �� -	   5        $ StackZoneJoueur2Attack��51      $ Vertical=�69   $	��Top=�52      $��Center=�6+   53      �� 56   
   $��0��6   $��1��6!   $��Center=�61   $��Center=�6L   
���  ��$��Joueur 1��57   !   $��48(�61   $
��Bold"�6?   $��Lucida Calligraphy)�58       J� Effect 59      3  -System.Windows.Media.Effects.DropShadowEffect  5:         Color$ Black��6#      ShadowDepth$ 3Q�61   	   
BlurRadius$	 2Q�6A   5<      �� 5>   
   $��0��6   $��5��6!   $��Center=�61   $��Center=�6L   
���  ��$��Joueur 2��5?   !   $��48(�61   $
��Bold"�6?   $��Lucida Calligraphy)�5@       5A        5B      $ Black��6#   $ 3Q�61   $	 2Q�6A   5D      �� 5G   
   $��0��6   $
��BANC��6$   $��0��60   $��Bottom=�6=   $��Center=�6X   
��� ���$��28(�5H   "   $
��Bold"�60   $��Lucida Calligraphy)�6B    5I        5J      $	 Red��6#   $ 3Q�6/   $	 2Q�6?   5L      �� 5M   
   $��1��6   $��DEFENSE��6$   $��0��63   $��Bottom=�6@   $��Center=�6[   
��� ���$��28(�5N   "   $
��Bold"�60   $��Lucida Calligraphy)�6B    5O        5P      $	 Red��6#   $ 3Q�6/   $	 2Q�6?   5R      �� 5S   
   $��2��6   $��ATTAQUE��6$   $��0��63   $��Bottom=�6@   $��Center=�6[   
��� ���$��28(�5T   "   $
��Bold"�60   $��Lucida Calligraphy)�6B    5U        5V      $	 Red��6#   $ 3Q�6/   $	 2Q�6?   5X      �� 5Y   
   $��6��6   $
��BANC��6$   $��0��60   $��Bottom=�6=   $��Center=�6X   
��� ���$��28(�5Z   "   $
��Bold"�60   $��Lucida Calligraphy)�6B    5[        5\      $	 Red��6#   $ 3Q�6/   $	 2Q�6?   5^      �� 5_   
   $��5��6   $��DEFENSE��6$   $��0��63   $��Bottom=�6@   $��Center=�6[   
��� ���$��28(�5`   "   $
��Bold"�60   $��Lucida Calligraphy)�6B    5a        5b      $	 Red��6#   $ 3Q�6/   $	 2Q�6?   5d      �� 5e   
   $��4��6   $��ATTAQUE��6$   $��0��63   $��Bottom=�6@   $��Center=�6[   
��� ���$��28(�5f   "   $
��Bold"�60   $��Lucida Calligraphy)�6B    5g        5h      $	 Red��6#   $ 3Q�6/   $	 2Q�6?   5j      �� 5m   
   -
   5        
 ��Name$
 CartePiochée��5m      $��1��6&   $��0��63   $��15q�6C   �� 5q   
   $��5q�6   $	��Top=�6   $	��250��64   $	��250��6@   $��3��6M   ._�5r      �� $��2q�6   $	��250��6$   $	��250��60   
��� ���.+�5t      �� -   5         ��Name$ OrTextBlockJoueur1��5t   :   $��5q�6   
�������$��Center=�5u      $��Center=�61   
���  ��$��20(�5v   %   $
��Bold"�63   $��Joueur 1 : 20��6E   �� 5x      -   5        $ VieTextBlockJoueur1��5x   :   $��5q�6   
�������$��Center=�5y      $��Center=�61   
����  �$��20(�5z   &   $
��Bold"�64   $��Vies : 3��6F   �� 5}      -   5        $ OrTextBlockJoueur2��5}   :   $��5q�6   
�������$��Center=�5~      $��Center=�61   
���  ��$��20(�5   %   $
��Bold"�63   $��Joueur 2 : 20��6E   �� 5�      -   5        $ VieTextBlockJoueur2��5�   :   $��5q�6   
�������$��Center=�5�      $��Center=�61   
����  �$��20(�5�   &   $
��Bold"�64   $��Vies : 3��6F   5�      5�      �� 5�   
   
��� ���$��3��6'   $��1��67   $��Bottom=�6D   .+�5�      ��  �� TextAlignment$ Center=�6   $"��Action possible mon seigneur��6/   $	��300��6S   $��30��5�      $��15,5,5,5q�6   $
��Bold"�60   
���  ��$��15(�6S   $��Lucida Calligraphy)�5�      $��Center=�62   $��Center=�6M   �� 5�      -   5        
������$��1q�5�   -   $ ��Afficher les cartes mortes��68   $��Stretch=�5�      $��Bottom=�6-   $��Lucida Calligraphy)�6H   $	��250��5�   2   $��40��6>   $��16(�6J   �� 5�      -   5         ��Name$ btnFinDeManche��5�   4   
��� ���$��5q�6)   $��Fin de Manche��6L   $��Center=�5�   )   $��Bottom=�6F   $��Lucida Calligraphy)�5�      $	��250��6/   $��40��6;   $��16(�6G   �� 5�      -   5        
�������
���  ��$
��Bold"�5�   H   $��1q�6Z   $��Piocher (Joueur 1)��6e   $��Center=�5�   2   $��Bottom=�6O   $��Lucida Calligraphy)�5�      $	��250��6/   $��40��6;   $��16(�6G   �� 5�      -   5        
���z���
���  ��$
��Bold"�5�   ?   $��1q�6Q   $��Piocher (Joueur 2)��6\   $��Center=�5�   1   $��Bottom=�6N   $��Lucida Calligraphy)�5�      $	��250��6/   $��40��6;   $��16(�6G   �� 5�      -   5        $ MessageTextBlock��5�   #   $ Center=�5�      
�������$��Center=�6B   $��3��5�      $��2��6"   $
��Bold"�6/   $
��Wrap=�6A   $��17(�6U   
���  ��$��5q�5�   =   $��60��6H   $�� ��6T   $��Center=�5�      $��Lucida Calligraphy)�6/   �� 5�      -   5        $ NextCardPriceTextBlock��5�   o   $ Center=�5�      
�������$��Center=�6B   $��3��5�      $��2��6"   $
��Bold"�6/   $
��Wrap=�6A   $��17(�6U   $��5q�5�      $��50��6   $�� ��6*   $��Center=�62   $��Lucida Calligraphy)�6O   
���   �6�   5�      �� 5�   
   $��1��6   $��0��6   $	��160��6.   $	��680��6:   $��2��6G   
�������._�5�      �� -   5        $ CarteContainerJoueur1��5�      $ Vertical=�68   $	��Top=�6O   $��Center=�6g   5�      �� 5�   
   $��1��6   $��6��6   $	��160��6.   $	��680��6:   $��2��6G   
�������._�5�      �� -   5        $ CarteContainerJoueur2��5�      $ Vertical=�68   $	��Top=�6O   $��Center=�6g   5�      �� 5�   
   -   5        $ btnAbandonJoueur1��5�   _   $��0��6   
���  ��
�������$
��Bold"�6B   $��5q�6T   $��Abandonner (Joueur 1)��6z   $��Center=�6�   $��Center=�6�   $��Lucida Calligraphy)�6�   $	��250��6  $��40��6  $��16(�6+  �� 5�   
   -   5        $ btnAbandonJoueur2��5�   o   $��0��6   $��7��6   
���  ��
�������$
��Bold"�6R   $��5q�6d   $��Abandonner (Joueur 2)��6�   $��Center=�6�   $��Center=�6�   $��Lucida Calligraphy)�6  $	��250��6#  $��40��6/  $��16(�6;   � 5�   
   -   5          �Name$ 	MainFrame��5�        � NavigationUIVisibility$ Hidden=�6#   5�      5�      