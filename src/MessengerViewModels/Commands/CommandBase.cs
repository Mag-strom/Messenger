﻿/******************************************************************************
* Filename    = CommandBase.cs
*
* Author      = Vinay Ingle
*
* Roll Number = 112001050
*
* Product     = Messenger
* 
* Project     = ViewModels
*
* Description = Base Class for commands.
* *****************************************************************************/


using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MessengerViewModels.Commands
{
    public abstract class CommandBase : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public virtual bool CanExecute(object parameter) => true;
         
        public abstract void Execute(object parameter);

        protected void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, new EventArgs());
        }
    }
}
