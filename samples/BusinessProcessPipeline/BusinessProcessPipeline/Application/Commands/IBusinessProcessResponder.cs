// Copyright 2020 Energinet DataHub A/S
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

using System.Threading.Tasks;

namespace BusinessProcessPipeline.Application.Commands
{
    public interface IBusinessProcessResponder<in TProcess>
        where TProcess : IBusinessRequest
    {
        Task RespondAsync(TProcess process, BusinessProcessResult businessProcessResult);
    }
    
    public class MyBusinessRequestResponder : IBusinessProcessResponder<MyBusinessRequest>
    {
        public Task RespondAsync(MyBusinessRequest process, BusinessProcessResult businessProcessResult)
        {
            // Based on the result of the BusinessProcessResult, generate and dispatch proper
            // Ebix/CIM reject, confirm and orther types of messages
            return Task.CompletedTask;
        }
    }
}