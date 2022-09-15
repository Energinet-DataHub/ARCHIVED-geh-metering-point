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

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Energinet.DataHub.MeteringPoints.Tests
{
    public class RolesTests
    {
        [Fact]
        public void Test()
        {
            var input = "14,27";

            var output = new List<string>();
            var rolesByNumber = input.Split(',');

            foreach (var number in rolesByNumber)
            {
                switch (number)
                {
                    case "14":
                        output.Add("A");
                        break;
                    case "27":
                        output.Add("B");
                        break;
                }
            }

            Assert.Equal("A,B", string.Join(',', output));
        }
    }
}
