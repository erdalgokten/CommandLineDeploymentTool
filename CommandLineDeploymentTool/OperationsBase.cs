﻿using CommandLineDeploymentTool.Helpers;
using System;
using System.IO;

namespace CommandLineDeploymentTool
{
    class OperationsBase : IOperations
    {
        protected Arguments args;
        protected OperationStep step = OperationStep.NONE;

        public OperationsBase(Arguments args)
        {
            this.args = args;
        }

        public virtual void ValidateArguments()
        {
            if (string.IsNullOrEmpty(this.args.DeployType))
                throw new ArgumentNullException("deployType", "deployType can not be null or empty");

            if (!this.args.NoBackup && !this.args.Restore && string.IsNullOrEmpty(this.args.BackupFolder))
                throw new ArgumentNullException("backupFolder", "backupFolder can not be null or empty");

            if (string.IsNullOrEmpty(this.args.AppName))
                throw new ArgumentNullException("appName", "appName can not be null or empty");

            if (string.IsNullOrEmpty(this.args.AppFolder))
                throw new ArgumentNullException("appFolder", "appFolder can not bu null or empty");

            if (!this.args.Restore && string.IsNullOrEmpty(this.args.DeployFolder))
                throw new ArgumentNullException("deployFolder", "deployFolder can not be null or empty");

            if (this.args.Restore && string.IsNullOrEmpty(this.args.RestorePath))
                throw new ArgumentNullException("restorePath", "restorePath can not be null or empty");

            if (!Directory.Exists(this.args.AppFolder))
                throw new ArgumentException((this.args.AppFolder ?? string.Empty) + " folder does not exist", "appFolder");

            if (!this.args.Restore && !Directory.Exists(this.args.DeployFolder))
                throw new ArgumentException((this.args.DeployFolder ?? string.Empty) + " folder does not exist", "deployFolder");

            if (this.args.Restore && !Directory.Exists(this.args.RestorePath))
                throw new ArgumentException((this.args.RestorePath ?? string.Empty) + " folder does not exist", "restorePath");
        }

        public virtual void Backup()
        {
            string destDir = Path.Combine(this.args.BackupFolder, this.args.AppName);
            destDir = Path.Combine(destDir, DateTime.Now.ToString("Backup_yyyyMMdd_HHmmss"));
            this.args.RestorePath = destDir;
            CopyHelper.DirectoryCopy(this.args.AppFolder, destDir, true);
        }

        public virtual void Restore()
        {
            CopyHelper.DirectoryCopy(this.args.RestorePath, this.args.AppFolder, true);
        }

        public virtual void Stop()
        {
            throw new NotImplementedException();
        }

        public virtual void Deploy()
        {
            CopyHelper.DirectoryCopy(this.args.DeployFolder, this.args.AppFolder, true);
        }

        public virtual void Start()
        {
            throw new NotImplementedException();
        }

        public virtual void Execute()
        {
            this.args.Restore = true;

            if (!this.args.NoBackup)
            {
                this.Backup();
                Console.WriteLine(this.args.DeployType + " backup complete...");
            }
            step = OperationStep.BACKEDUP;

            if (!this.args.NoStop)
            {
                this.Stop();
                Console.WriteLine(this.args.DeployType + " stop complete...");
            }
            step = OperationStep.STOPPED;

            this.Deploy();
            Console.WriteLine(this.args.DeployType + " deploy complete...");
            step = OperationStep.DEPLOYED;

            if (!this.args.NoStart)
            {
                this.Start();
                Console.WriteLine(this.args.DeployType + " start complete...");
            }
            step = OperationStep.STARTED;
        }

        public virtual void Rollback()
        {
            if (this.args.Restore)
                this.step = OperationStep.STARTED;

            if (this.step == OperationStep.NONE || this.step == OperationStep.BACKEDUP)
            {
                Console.WriteLine("No rollback required");
                return;
            }

            if (this.step == OperationStep.STARTED && !this.args.NoStart)
            {
                this.Stop();
                Console.WriteLine(this.args.DeployType + " stop complete...");
            }

            if (this.step == OperationStep.STARTED || this.step == OperationStep.DEPLOYED)
            {
                this.Restore();
                Console.WriteLine(this.args.DeployType + " restore complete...");
            }

            if ((this.step == OperationStep.STARTED || this.step == OperationStep.DEPLOYED || this.step == OperationStep.STOPPED) && !this.args.NoStop)
            {
                this.Start();
                Console.WriteLine(this.args.DeployType + " start complete...");
            }
        }
    }
}
