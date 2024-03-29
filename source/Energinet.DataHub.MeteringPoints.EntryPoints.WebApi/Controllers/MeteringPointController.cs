﻿// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using System.Threading.Tasks;
using Energinet.DataHub.MeteringPoints.Client.Abstractions.Models;
using Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.MeteringPoints.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MeteringPointController : ControllerBase
    {
        private readonly IMediator _mediator;

        public MeteringPointController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetMeteringPointByGsrn")]
        public async Task<ActionResult<MeteringPointCimDto?>> GetMeteringPointByGsrnAsync(string gsrn)
        {
            var request = new MeteringPointByGsrnQuery(gsrn);
            var result = await _mediator.Send(request).ConfigureAwait(false);

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpGet("GetMeteringPointProcessesByGsrn")]
        public async Task<ActionResult<List<Process>>> GetProcessesByGsrnAsync(string gsrn)
        {
            var request = new MeteringPointProcessesByGsrnQuery(gsrn);
            var result = await _mediator.Send(request).ConfigureAwait(false);

            return Ok(result);
        }
    }
}
