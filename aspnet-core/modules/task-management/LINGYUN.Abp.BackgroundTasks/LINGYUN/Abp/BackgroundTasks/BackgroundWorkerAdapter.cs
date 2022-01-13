﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.DynamicProxy;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Timing;

namespace LINGYUN.Abp.BackgroundTasks;

public class BackgroundWorkerAdapter<TWorker> : BackgroundWorkerBase, IBackgroundWorkerRunnable
    where TWorker : IBackgroundWorker
{
    protected IClock Clock => LazyServiceProvider.LazyGetRequiredService<IClock>();
    protected IJobStore JobStore => LazyServiceProvider.LazyGetRequiredService<IJobStore>();
    protected IJobScheduler JobScheduler => LazyServiceProvider.LazyGetRequiredService<IJobScheduler>();
    protected ICurrentTenant CurrentTenant => LazyServiceProvider.LazyGetRequiredService<ICurrentTenant>();

    private readonly MethodInfo _doWorkAsyncMethod;
    private readonly MethodInfo _doWorkMethod;

    public BackgroundWorkerAdapter()
    {
        _doWorkAsyncMethod = typeof(TWorker).GetMethod("DoWorkAsync", BindingFlags.Instance | BindingFlags.NonPublic);
        _doWorkMethod = typeof(TWorker).GetMethod("DoWork", BindingFlags.Instance | BindingFlags.NonPublic);
    }

#nullable enable
    public JobInfo? BuildWorker(IBackgroundWorker worker)
    {
        int? period;
        var workerType = ProxyHelper.GetUnProxiedType(worker);

        if (worker is AsyncPeriodicBackgroundWorkerBase or PeriodicBackgroundWorkerBase)
        {
            if (typeof(TWorker) != workerType)
            {
                throw new ArgumentException($"{nameof(worker)} type is different from the generic type");
            }

            var timer = workerType.GetProperty("Timer", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(worker);
            if (timer == null)
            {
                return null;
            }

            if (worker is AsyncPeriodicBackgroundWorkerBase)
            {
                period = ((AbpAsyncTimer)timer)?.Period;
            }
            else
            {
                period = ((AbpTimer)timer)?.Period;
            }
        }
        else
        {
            return null;
        }

        if (period == null)
        {
            return null;
        }

        var jobArgs = new Dictionary<string, object>
        {
            { "JobType", workerType.AssemblyQualifiedName },
        };
        return new JobInfo
        {
            Id = workerType.FullName,
            TenantId = CurrentTenant.Id,
            Name = workerType.FullName,
            Group = "BackgroundWorkers",
            Priority = JobPriority.Normal,
            BeginTime = Clock.Now,
            Args = jobArgs,
            Description = "From the framework background workers",
            JobType = JobType.Persistent,
            Interval = period.Value / 1000,
            MaxCount = 0,
            // TODO: 可配置
            MaxTryCount = 10,
            CreationTime = Clock.Now,
            // 确保不会被轮询入队
            Status = JobStatus.None,
            Type = typeof(BackgroundWorkerAdapter<TWorker>).AssemblyQualifiedName,
        };
    }
#nullable disable

    public async Task ExecuteAsync(JobRunnableContext context)
    {
        var worker = (IBackgroundWorker)ServiceProvider.GetService(typeof(TWorker));
        var workerContext = new PeriodicBackgroundWorkerContext(ServiceProvider);

        switch (worker)
        {
            case AsyncPeriodicBackgroundWorkerBase asyncWorker:
                {
                    if (_doWorkAsyncMethod != null)
                    {
                        await(Task) _doWorkAsyncMethod.Invoke(asyncWorker, new object[] { workerContext });
                    }

                    break;
                }
            case PeriodicBackgroundWorkerBase syncWorker:
                {
                    _doWorkMethod?.Invoke(syncWorker, new object[] { workerContext });

                    break;
                }
        }
    }
}
