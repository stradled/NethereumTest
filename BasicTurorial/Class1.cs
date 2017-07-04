using System.Threading;
using System.Threading.Tasks;
using Nethereum.Geth;
using Nethereum.Hex.HexTypes;
using Nethereum.Web3;
using Xunit;

namespace BasicTurorial
{
    public class TestClass
    {
        [Fact]
        public async Task ShouldBeAbleToDeployAContract()
        {
            var senderAddress = "0x12890d2cce102216644c59daE5baed380d84830c";
            var password = "password";

            var abi = @"[{""constant"":false,""inputs"":[{""name"":""val"",""type"":""int256""}],""name"":""multiply"",""outputs"":[{""name"":""d"",""type"":""int256""}],""payable"":false,""type"":""function""},{""inputs"":[{""name"":""multiplier"",""type"":""int256""}],""payable"":false,""type"":""constructor""}]";
            var byteCode =
                "0x60606040523415600b57fe5b6040516020806100ac83398101604052515b60008190555b505b6079806100336000396000f300606060405263ffffffff60e060020a6000350416631df4f14481146020575bfe5b3415602757fe5b60306004356042565b60408051918252519081900360200190f35b60005481025b9190505600a165627a7a72305820ec2cf1c300fa92e0ccd1f5ce518cc43c9578fe9007797024f1424d07f491020a0029";
            var multiplier = 7;

            var web3 = new Web3();
            var web3Geth = new Web3Geth();

            var unlockAccountResult =
                await web3.Personal.UnlockAccount.SendRequestAsync(senderAddress, password, 120);

            Assert.True(unlockAccountResult);

            var transactionHash =
                await web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, senderAddress, new HexBigInteger(250000),
                    multiplier);

            var mineResult = await web3Geth.Miner.Start.SendRequestAsync(6);
            //var mineResult = await web3.Miner.Start.SendRequestAsync(6);

            Assert.True(mineResult);

            var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

            while (receipt == null)
            {
                Thread.Sleep(5000);
                receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
            }

            var contractAddress = receipt.ContractAddress;
            var contract = web3.Eth.GetContract(abi, contractAddress);

            var multiplyFunction = contract.GetFunction("multiply");

            var result = await multiplyFunction.CallAsync<int>(7);

            Assert.Equal(49, result);
        }
    }
}