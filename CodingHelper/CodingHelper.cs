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
            _helperCommands = new Dictionary<string, HelperCommand>();
        }

        /// <summary>实现 IDTExtensibility2 接口的 OnConnection 方法。接收正在加载外接程序的通知。</summary>
        /// <param term='application'>宿主应用程序的根对象。</param>
        /// <param term='connectMode'>描述外接程序的加载方式。</param>
        /// <param term='addInInst'>表示此外接程序的对象。</param>
        /// <seealso class='IDTExtensibility2' />
        public void OnConnection(object application, ext_ConnectMode connectMode, object addInInst, ref Array custom)
        {
            _applicationObject = (DTE2)application;
            _addInInstance = (AddIn)addInInst;

            if (connectMode == ext_ConnectMode.ext_cm_UISetup)
            {
                InitHelperCommands();
            }
            else if (connectMode == ext_ConnectMode.ext_cm_Startup)
            {
                Logger.Initialize(_applicationObject);

                foreach (HelperCommand cmd in GetAllHelperCommands())
                {
                    _helperCommands.Add(_addInInstance.ProgID + "." + cmd.Name, cmd);
                }
            }
        }

        private void InitHelperCommands()
        {
            Commands2 allCommands = (Commands2)_applicationObject.Commands;
            string toolsMenuName = "Tools";

            CommandBarPopup toolsPopup = GetTopMenuBar(toolsMenuName);
            object[] contextGUIDS = new object[] { };

            foreach (HelperCommand cmd in GetAllHelperCommands())
            {
                try
                {
                    Command command = allCommands.AddNamedCommand2(
                        _addInInstance, cmd.Name, cmd.Text, cmd.Description,
                        true, 59, ref contextGUIDS,
                        (int)vsCommandStatus.vsCommandStatusSupported +
                        (int)vsCommandStatus.vsCommandStatusEnabled,
                        (int)vsCommandStyle.vsCommandStylePictAndText,
                        vsCommandControlType.vsCommandControlTypeButton);

                    if ((command != null) && (toolsPopup != null))
                    {
                        command.AddControl(toolsPopup.CommandBar, 1);
                    }
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
            CommandBar bar = ((CommandBars)_applicationObject.CommandBars)["MenuBar"];
            CommandBarControl barCtrl = bar.Controls[toplevelMenu];
            CommandBarPopup barPopup = (CommandBarPopup)barCtrl;

            Logger.Initialize(_applicationObject);

            foreach (CommandBarControl c in barPopup.Controls)
            {
                Logger.WriteMessage(c.Caption + ": " + c.Index);
            }

            object index = null;
            barCtrl = barPopup.Controls["导入和导出设置..."];
            if (barCtrl != null) index = barCtrl.Index;

            barCtrl = barPopup.Controls.Add(MsoControlType.msoControlPopup, null, null, index);
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

            if (executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault)
            {
                HelperCommand cmd;

                if (_helperCommands.TryGetValue(commandName, out cmd))
                {
                    handled = true;
                    varOut = cmd.Execute(_applicationObject, _addInInstance, varIn);
                }
            }
        }

        private DTE2 _applicationObject;
        private AddIn _addInInstance;
        private Dictionary<string, HelperCommand> _helperCommands;
    }
}


