﻿using System.Linq;
using AutoMapper;
using TaskTracking.TaskGroupAggregate.Dtos.TaskGroups;
using TaskTracking.TaskGroupAggregate.Dtos.TaskItems;
using TaskTracking.TaskGroupAggregate.Dtos.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.TaskGroups;
using TaskTracking.TaskGroupAggregate.TaskItems;
using TaskTracking.TaskGroupAggregate.UserTaskGroups;
using TaskTracking.TaskGroupAggregate.UserTaskProgresses;

namespace TaskTracking;

public class TaskTrackingApplicationAutoMapperProfile : Profile
{
    public TaskTrackingApplicationAutoMapperProfile()
    {
        // TaskGroup mappings
        CreateMap<TaskGroup, TaskGroupDto>()
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src =>
                src.Tasks.Count > 0 && src.Tasks.All(t => t.UserProgresses.Any(up => up.ProgressPercentage == 100))));

        CreateMap<TaskGroup, TaskGroupDetailsDto>()
            .ForMember(dest => dest.Tasks, opt => opt.MapFrom(src => src.Tasks))
            .ForMember(dest => dest.UserTaskGroups, opt => opt.MapFrom(src => src.UserTaskGroups))
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src =>
                src.Tasks.Count > 0 && src.Tasks.All(t => t.UserProgresses.Any(up => up.ProgressPercentage == 100))));

        // TaskItem mappings
        CreateMap<TaskItem, TaskItemDto>()
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src =>
                src.UserProgresses.Any(up => up.ProgressPercentage == 100)));

        // UserTaskGroup mappings
        CreateMap<UserTaskGroup, UserTaskGroupDto>();

        // UserTaskProgress mappings
        CreateMap<UserTaskProgress, UserTaskProgressDto>();
        CreateMap<ProgressEntry, ProgressEntryDto>();

        // RecurrencePattern mappings
        CreateMap<RecurrencePattern, RecurrencePatternDto>()
            .ForMember(dest => dest.DaysOfWeek,
                opt => opt.MapFrom(src => src.DaysOfWeek.ToList()));

        CreateMap<CreateRecurrencePatternDto, RecurrencePattern>()
            .ConstructUsing((src, ctx) =>
            {
                switch (src.RecurrenceType)
                {
                    case RecurrenceType.Daily:
                        return RecurrencePattern.CreateDaily(src.Interval, src.EndDate, src.Occurrences);
                    case RecurrenceType.Weekly:
                        return RecurrencePattern.CreateWeekly(src.Interval, src.DaysOfWeek, src.EndDate,
                            src.Occurrences);
                    case RecurrenceType.Monthly:
                        return RecurrencePattern.CreateMonthly(src.Interval, src.EndDate, src.Occurrences);
                    default:
                        return null;
                }
            });
    }
}