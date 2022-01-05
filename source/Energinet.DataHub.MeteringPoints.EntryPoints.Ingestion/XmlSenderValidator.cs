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

using Energinet.DataHub.Core.FunctionApp.Common.Abstractions.Identity;
using Energinet.DataHub.Core.XmlConversion.XmlConverter.Abstractions;

namespace Energinet.DataHub.MeteringPoints.EntryPoints.Ingestion
{
    public class XmlSenderValidator
    {
        private readonly IUserContext _userContext;

        public XmlSenderValidator(IUserContext userContext)
        {
            _userContext = userContext;
        }

        public (bool IsValid, string ErrorMessage) ValidateSender(XmlHeaderSender? sender)
        {
            if (_userContext.CurrentUser is null)
                return (false, "User is not authorized.");

            return sender?.Id != _userContext.CurrentUser.Identifier
                ? (false, "Identifier applied for sender was not equal to the identifier of the authorized actor.")
                : (true, string.Empty);
        }
    }
}
