using System;
using Extensibility;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.CommandBars;
using System.Collections.Generic;

namespace Blacksand
{
    /// <summary>用于实现外接程序的对象。</summary>
    /// <seealso class='IDTExtensibility2' />
    public class CodingHelper : IDTExtensibility2, IDTCommandTarget
    {
        /// <summary>实现外接程序对象的构造函数。请将您的初始化代码置于此方法内。</summary>
        public CodingHelper()
        {
        }

        /// <summary>实现 IDTExtensibility2 接口的 OnConnection 方法。接收正在加载外接程序的通知。</summary>
        /// <param term='application'>宿主应用程序的根对象。</param>
        /// <param term='connectMode'>描述外接程序的加载方式。</param>
        /// <param term='addInInst'>表示此外接程序的对象。</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _appObject = (DTE2)application;
            _addIn = (AddIn)addInInst;

            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                // 在这里创建 NamedCommand2 对象, 不要关联到 Menu
                InitHelperCommands();
            }
            else if (connectMode == ext_ConnectMode.ext_cm_Startup)
            {
                // 在这里取出 NamedCommand2, 并创建 Menu 关联
                Logger.Initialize(_appObject);
                string toolsMenuName = "Tools";
                CommandBarPopup toolsPopup = GetTopMenuBar(toolsMenuName);
                _helperCommands = new Dictionary<string, HelperCommand>();

                foreach (HelperCommand cmd in GetAllHelperCommands())
                {
                    string fullName = _addIn.ProgID + "." + cmd.Name;
                    cmd.NamedCommand = _appObject.Commands.Item(fullName);
                    _helperCommands.Add(fullName, cmd);

                    if ((cmd.NamedCommand != null) && (toolsPopup != null))
                    {
                        cmd.NamedCommand.AddControl(toolsPopup.CommandBar, toolsPopup.Controls.Count + 1);
                    }
                }
            }
        }

        private void InitHelperCommands()
        {
            Commands2 allCommands = (Commands2)_appObject.Commands;
            object[] contextGUIDS = new object[] { };

            foreach (HelperCommand cmd in GetAllHelperCommands())
            {
                try
                {
                    Command command = allCommands.AddNamedCommand2(
                        _addIn, cmd.Name, cmd.Text, cmd.Description,
                        true, 59, ref contextGUIDS,
                        (int)vsCommandStatus.vsCommandStatusSupported +
                        (int)vsCommandStatus.vsCommandStatusEnabled,
                        (int)vsCommandStyle.vsCommandStylePictAndText,
                        vsCommandControlType.vsCommandControlTypeButton);

                }
                catch (System.ArgumentException)
                { }
            }
        }

        private HelperCommand[] GetAllHelperCommands()
        {
            HelperCommand[] allCmds = 
            {
                new FormatCodeCommand(),
            };
            return allCmds;
        }

        private CommandBarPopup GetTopMenuBar(string toplevelMenu)
        {
            CommandBar bar = ((CommandBars)_appObject.CommandBars)["MenuBar"];
            CommandBarControl barCtrl = bar.Controls[toplevelMenu];
            CommandBarPopup barPopup = (CommandBarPopup)barCtrl;

            object index = null;
            barCtrl = barPopup.Controls["Import and Export Settings..."];
            if (barCtrl != null) index = barCtrl.Index;

            barCtrl = barPopup.Controls.Add(MsoControlType.msoControlPopup, index, null, index);
            barCtrl.BeginGroup = true;
            barCtrl.Caption = "CodingHelper";

            barPopup = (CommandBarPopup)barCtrl;
            return barPopup;
        }

        /// <summary>实现 IDTExtensibility2 接口的 OnDisconnection 方法。接收正在卸载外接程序的通知。</summary>
        /// <param term='disconnectMode'>描述外接程序的卸载方式。</param>
        /// <param term='custom'>特定于宿主应用程序的参数数组。</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnDisconnection(ext_DisconnectMode disconnectMode, ref Array custom)
        {
        }

        /// <summary>实现 IDTExtensibility2 接口的 OnAddInsUpdate 方法。当外接程序集合已发生更改时接收通知。</summary>
        /// <param term='custom'>特定于宿主应用程序的参数数组。</param>
        /// <seealso class='IDTExtensibility2' />		
        public void OnAddInsUpdate(ref Array custom)
        {
        }

        /// <summary>实现 IDTExtensibility2 接口的 OnStartupComplete 方法。接收宿主应用程序已完成加载的通知。</summary>
        /// <param term='custom'>特定于宿主应用程序的参数数组。</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnStartupComplete(ref Array custom)
        {
        }

        /// <summary>实现 IDTExtensibility2 接口的 OnBeginShutdown 方法。接收正在卸载宿主应用程序的通知。</summary>
        /// <param term='custom'>特定于宿主应用程序的参数数组。</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnBeginShutdown(ref Array custom)
        {
        }

        /// <summary>
        /// 实现 IDTCommandTarget 接口的 QueryStatus 方法。
        /// </summary>
        /// <param name="commandName"></param>
        /// <param name="neededText"></param>
        /// <param name="status"></param>
        /// <param name="commandText"></param>
        public void QueryStatus(string commandName, vsCommandStatusTextWanted neededText,
                                ref vsCommandStatus status, ref object commandText)
        {
            if (neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone)
            {
                HelperCommand cmd;
                if (_helperCommands.TryGetValue(commandName, out cmd))
                {
                    status = (vsCommandStatus)vsCommandStatus.vsCommandStatusSupported
                        | vsCommandStatus.vsCommandStatusEnabled;
                    commandText = cmd.Text;
                    return;
                }
            }
        }

        /// <summary> /// 实现 IDTCommandTarget 接口的 Exec 方法。 </summary>
        /// <param name="commandName"></param>
        /// <param name="executeOption"></param>
        /// <param name="varIn"></param>
        /// <param name="varOut"></param>
        /// <param name="handled"></param>
        public void Exec(string commandName, vsCommandExecOption executeOption,
            ref object varIn, ref object varOut, ref bool handled)
        {
            handled = false;
            Logger.Initialize(_appObject);

            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                HelperCommand cmd;

                if (_helperCommands.TryGetValue(commandName, out cmd))
                {
                    handled = true;
                    varOut = cmd.Execute(_appObject, _addIn, varIn);
                }
            }
        }

        private DTE2 _appObject;
        private AddIn _addIn;
        private Dictionary<string, HelperCommand> _helperCommands;
    }
}


