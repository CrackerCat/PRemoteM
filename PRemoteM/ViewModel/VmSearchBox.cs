﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using PRM.Core;
using PRM.Core.Model;
using PRM.Core.Protocol;
using Shawn.Utils;
using Shawn.Utils.PageHost;

namespace PRM.ViewModel
{
    public class ActionItem : NotifyPropertyChangedBase
    {
        private string _actionName = "";
        public string ActionName
        {
            get => _actionName;
            set => SetAndNotifyIfChanged(nameof(ActionName), ref _actionName, value);
        }

        public Action Run;
    }




    public class VmSearchBox : NotifyPropertyChangedBase
    {
        private readonly double _oneItemHeight;
        private readonly double _oneActionHeight;
        private readonly double _cornerRadius;
        private readonly FrameworkElement _listSelections;
        private readonly FrameworkElement _listActions;

        public VmSearchBox(double oneItemHeight, double oneActionHeight, double cornerRadius, FrameworkElement listSelections, FrameworkElement listActions)
        {
            _oneItemHeight = oneItemHeight;
            _oneActionHeight = oneActionHeight;
            this._listSelections = listSelections;
            this._listActions = listActions;
            _cornerRadius = cornerRadius;
            GridKeywordHeight = 46;
            GridMainWidth = 400;
            RecalcWindowHeight(false);
        }


        private VmProtocolServer _selectedItem;
        public VmProtocolServer SelectedItem
        {
            get => _selectedItem;
            private set => SetAndNotifyIfChanged(nameof(SelectedItem), ref _selectedItem, value);
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                SetAndNotifyIfChanged(nameof(SelectedIndex), ref _selectedIndex, value);
                if (GlobalData.Instance.VmItemList.Count > 0
                    && _selectedIndex >= 0
                    && _selectedIndex < GlobalData.Instance.VmItemList.Count)
                {
                    SelectedItem = GlobalData.Instance.VmItemList[_selectedIndex];
                }
                else
                {
                    SelectedItem = null;
                }
            }
        }



        private ObservableCollection<ActionItem> _actions = new ObservableCollection<ActionItem>();
        public ObservableCollection<ActionItem> Actions
        {
            get => _actions;
            set => SetAndNotifyIfChanged(nameof(Actions), ref _actions, value);
        }

        private int _selectedActionIndex;
        public int SelectedActionIndex
        {
            get => _selectedActionIndex;
            set => SetAndNotifyIfChanged(nameof(SelectedActionIndex), ref _selectedActionIndex, value);
        }


        private string _dispNameFilter;
        public string DispNameFilter
        {
            get => _dispNameFilter;
            set
            {
                SetAndNotifyIfChanged(nameof(DispNameFilter), ref _dispNameFilter, value);
                UpdateItemsList(value);
            }
        }



        private int _displayItemCount;
        public int DisplayItemCount
        {
            get => _displayItemCount;
            set => SetAndNotifyIfChanged(nameof(DisplayItemCount), ref _displayItemCount, value);
        }


        private double _gridMainWidth;
        public double GridMainWidth
        {
            get => _gridMainWidth;
            set
            {
                SetAndNotifyIfChanged(nameof(GridMainWidth), ref _gridMainWidth, value);
                GridMainClip = new RectangleGeometry(new Rect(new Size(GridMainWidth, GridMainHeight)), _cornerRadius, _cornerRadius);
            }
        }


        private double _gridMainHeight;
        public double GridMainHeight
        {
            get => _gridMainHeight;
            set
            {
                SetAndNotifyIfChanged(nameof(GridMainHeight), ref _gridMainHeight, value);
                GridMainClip = new RectangleGeometry(new Rect(new Size(GridMainWidth, GridMainHeight)), _cornerRadius, _cornerRadius);
            }
        }

        private RectangleGeometry _gridMainClip = null;
        public RectangleGeometry GridMainClip
        {
            get => _gridMainClip;
            set => SetAndNotifyIfChanged(nameof(GridMainClip), ref _gridMainClip, value);
        }

        public double GridKeywordHeight { get; }


        private double _gridSelectionsHeight;
        public double GridSelectionsHeight
        {
            get => _gridSelectionsHeight;
            set => SetAndNotifyIfChanged(nameof(GridSelectionsHeight), ref _gridSelectionsHeight, value);
        }


        private double _gridActionsHeight;
        public double GridActionsHeight
        {
            get => _gridActionsHeight;
            set => SetAndNotifyIfChanged(nameof(GridActionsHeight), ref _gridActionsHeight, value);
        }



        public void RecalcWindowHeight(bool showGridAction)
        {
            if (showGridAction)
            {
                GridSelectionsHeight = (Actions?.Count ?? 0) * _oneActionHeight;
                GridActionsHeight = GridKeywordHeight + GridSelectionsHeight;
                GridMainHeight = GridActionsHeight;
            }
            else
            {
                if (DisplayItemCount >= 8)
                {
                    GridSelectionsHeight = _oneItemHeight * 8;
                }
                else
                    GridSelectionsHeight = _oneItemHeight * DisplayItemCount;
                GridMainHeight = GridKeywordHeight + GridSelectionsHeight;
            }
        }

        public void ShowActionsList()
        {
            #region Build Actions
            var actions = new ObservableCollection<ActionItem>();
            actions.Add(new ActionItem()
            {
                ActionName = SystemConfig.Instance.Language.GetText("server_card_operate_conn"),
                Run = () =>
                {
                    Debug.Assert(SelectedItem?.Server != null);
                    GlobalEventHelper.OnServerConnect?.Invoke(SelectedItem.Server.Id);
                },
            });
            actions.Add(new ActionItem()
            {
                ActionName = SystemConfig.Instance.Language.GetText("server_card_operate_edit"),
                Run = () =>
                {
                    Debug.Assert(SelectedItem?.Server != null);
                    GlobalEventHelper.OnGoToServerEditPage?.Invoke(SelectedItem.Server.Id, false);
                },
            });
            actions.Add(new ActionItem()
            {
                ActionName = SystemConfig.Instance.Language.GetText("server_card_operate_duplicate"),
                Run = () =>
                {
                    Debug.Assert(SelectedItem?.Server != null);
                    GlobalEventHelper.OnGoToServerEditPage?.Invoke(SelectedItem.Server.Id, true);
                },
            });
            if (SelectedItem.Server.GetType().IsSubclassOf(typeof(ProtocolServerWithAddrPortBase)))
            {
                actions.Add(new ActionItem()
                {
                    ActionName = SystemConfig.Instance.Language.GetText("server_card_operate_copy_address"),
                    Run = () =>
                    {
                        if (SelectedItem.Server is ProtocolServerWithAddrPortBase server)
                            Clipboard.SetText($"{server.Address}:{server.GetPort()}");
                    },
                });
            }
            if (SelectedItem.Server.GetType().IsSubclassOf(typeof(ProtocolServerWithAddrPortUserPwdBase)))
            {
                actions.Add(new ActionItem()
                {
                    ActionName = SystemConfig.Instance.Language.GetText("server_card_operate_copy_username"),
                    Run = () =>
                    {
                        if (SelectedItem.Server is ProtocolServerWithAddrPortUserPwdBase server)
                            Clipboard.SetText(server.UserName);
                    },
                });
            }
            if (SelectedItem.Server.GetType().IsSubclassOf(typeof(ProtocolServerWithAddrPortUserPwdBase)))
            {
                actions.Add(new ActionItem()
                {
                    ActionName = SystemConfig.Instance.Language.GetText("server_card_operate_copy_password"),
                    Run = () =>
                    {
                        if (SelectedItem.Server is ProtocolServerWithAddrPortUserPwdBase server)
                            Clipboard.SetText(server.GetDecryptedPassWord());
                    },
                });
            } 
            #endregion

            Actions = actions;
            SelectedActionIndex = 0;

            RecalcWindowHeight(true);

            _listActions.Visibility = Visibility.Visible;

            var sb = AnimationPage.GetInOutStoryboard(0.3,
                AnimationPage.InOutAnimationType.SlideFromLeft,
                GridMainWidth,
                GridMainHeight);
            sb.Begin(_listActions);
        }

        public void HideActionsList()
        {
            var sb = AnimationPage.GetInOutStoryboard(0.3,
                AnimationPage.InOutAnimationType.SlideToLeft,
                GridMainWidth,
                GridMainHeight);
            sb.Completed += (o, args) =>
            {
                _listActions.Visibility = Visibility.Hidden;
                RecalcWindowHeight(false);
            };
            sb.Begin(_listActions);
        }

        private void UpdateItemsList(string keyword)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                var keyWords = keyword.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                var keyWordIsMatch = new List<bool>(keyWords.Length);
                for (var i = 0; i < keyWords.Length; i++)
                    keyWordIsMatch.Add(false);

                int nMatchedCount = 0;
                // match keyword
                foreach (var item in GlobalData.Instance.VmItemList.Where(x =>
                    x.GetType() != typeof(ProtocolServerNone)))
                {
                    Debug.Assert(item != null);
                    Debug.Assert(!string.IsNullOrEmpty(item.Server.ClassVersion));
                    Debug.Assert(!string.IsNullOrEmpty(item.Server.Protocol));

                    var dispName = item.Server.DispName;
                    var subTitle = item.Server.SubTitle;


                    var mDispName = new List<List<bool>>();
                    var mSubTitle = new List<List<bool>>();
                    for (var i = 0; i < keyWordIsMatch.Count; i++)
                    {
                        var f1 = dispName.IsMatchPinyinKeywords(keyWords[i], out var m1);
                        var f2 = subTitle.IsMatchPinyinKeywords(keyWords[i], out var m2);
                        mDispName.Add(m1);
                        mSubTitle.Add(m2);
                        keyWordIsMatch[i] = f1 || f2;
                    }

                    if (keyWordIsMatch.All(x => x == true))
                    {
                        var m1 = new List<bool>();
                        var m2 = new List<bool>();
                        for (var i = 0; i < dispName.Length; i++)
                            m1.Add(false);
                        for (var i = 0; i < subTitle.Length; i++)
                            m2.Add(false);
                        for (var i = 0; i < keyWordIsMatch.Count; i++)
                        {
                            if (mDispName[i] != null)
                                for (int j = 0; j < mDispName[i].Count; j++)
                                    m1[j] |= mDispName[i][j];
                            if (mSubTitle[i] != null)
                                for (int j = 0; j < mSubTitle[i].Count; j++)
                                    m2[j] |= mSubTitle[i][j];
                        }


                        item.ObjectVisibility = Visibility.Visible;
                        ++nMatchedCount;
                        const bool enableHighLine = true;
                        if (enableHighLine)
                        {
                            if (m1.Any(x => x == true))
                            {
                                var sp = new StackPanel()
                                { Orientation = System.Windows.Controls.Orientation.Horizontal };
                                for (int i = 0; i < m1.Count; i++)
                                {
                                    if (m1[i])
                                        sp.Children.Add(new TextBlock()
                                        {
                                            Text = dispName[i].ToString(),
                                            Background = new SolidColorBrush(Color.FromArgb(80, 239, 242, 132)),
                                        });
                                    else
                                        sp.Children.Add(new TextBlock()
                                        {
                                            Text = dispName[i].ToString(),
                                        });
                                }

                                item.DispNameControl = sp;
                            }
                            if (m2.Any(x => x == true))
                            {
                                var sp = new StackPanel()
                                { Orientation = System.Windows.Controls.Orientation.Horizontal };
                                for (int i = 0; i < m2.Count; i++)
                                {
                                    if (m2[i])
                                        sp.Children.Add(new TextBlock()
                                        {
                                            Text = subTitle[i].ToString(),
                                            Background = new SolidColorBrush(Color.FromArgb(80, 239, 242, 132)),
                                        });
                                    else
                                        sp.Children.Add(new TextBlock()
                                        {
                                            Text = subTitle[i].ToString(),
                                        });
                                }

                                item.SubTitleControl = sp;
                            }
                        }
                    }
                    else
                    {
                        item.ObjectVisibility = Visibility.Collapsed;
                    }
                }
                DisplayItemCount = nMatchedCount;
            }
            else
            {
                // show all
                foreach (var item in GlobalData.Instance.VmItemList)
                {
                    item.ObjectVisibility = Visibility.Visible;
                    item.DispNameControl = item.OrgDispNameControl;
                    item.SubTitleControl = item.OrgSubTitleControl;
                }
                DisplayItemCount = GlobalData.Instance.VmItemList.Count;
            }

            // reorder
            for (var i = 1; i < GlobalData.Instance.VmItemList.Count; i++)
            {
                var s0 = GlobalData.Instance.VmItemList[i - 1];
                var s1 = GlobalData.Instance.VmItemList[i];
                if (s0.Server.LastConnTime < s1.Server.LastConnTime)
                {
                    GlobalData.Instance.VmItemList = new ObservableCollection<VmProtocolServer>(GlobalData.Instance.VmItemList.OrderByDescending(x => x.Server.LastConnTime));
                    break;
                }
            }


            // index the list to first item
            for (var i = 0; i < GlobalData.Instance.VmItemList.Count; i++)
            {
                var vmClipObject = GlobalData.Instance.VmItemList[i];
                if (vmClipObject.ObjectVisibility == Visibility.Visible)
                {
                    SelectedIndex = i;
                    SelectedItem = vmClipObject;
                    break;
                }
            }

            RecalcWindowHeight(false);
        }
    }
}
