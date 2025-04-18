﻿using System;
using AutoMapper;
using CompanyName.ProjectName.WebApi.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CompanyName.ProjectName.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [ServiceFilter(typeof(ApiExceptionFilter))]
    public abstract class BaseController<T> : ControllerBase
    {
        protected readonly ILogger<T> logger;
        protected readonly IMapper mapper;

        protected BaseController(ILogger<T> logger, IMapper mapper)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
    }
}
