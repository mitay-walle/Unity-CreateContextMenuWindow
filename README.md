# Unity Menu Item Windows
- Create Context Menu Window
Display 'Assets/Create/...' items as EditorWindow with hierarchy buttons
![alt text](https://github.com/mitay-walle/Unity-CreateContextMenuWindow/blob/master/Documentation/preview_0.png)
- Menu Item Window
Display all menu items 'File/..' 'Edit/..' 'CustoMenuitem/..' etc same way
![alt text](https://github.com/mitay-walle/Unity-CreateContextMenuWindow/blob/master/Documentation/preview_3.png)
- search example 
<br>![alt text](https://github.com/mitay-walle/Unity-CreateContextMenuWindow/blob/master/Documentation/preview_2.png)

# Problem

## I. Context Menu missclicks close
More grow list and depth it's became hard to not missclick, and close context menu, start over and over again
## II. Context menu nested navigation
nested nature of context menu is flexible for organization, but 'Hover-to-open' with big nested hierarchies became unusable

# Solution
Display menu items as EditorWindow with hierarchy buttons
<br>I. missclick is doesn't matter
<br>II. no need to Hover-to-navigate

# Summary
- UPM package
- search
- Create Context Menu Window for 'Assets/Create/..'
- Menu Item Window for all 'File/..' 'Edit/..' 'CustoMenuitem/..'
- customize style once, saved in EditorPrefs
- open with Windows/Create Context Menu Window
- scrollview for big amount of items, (saves its state!)
- change Indent and button size to customize window for your needs
