using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Fulu.Core.Extensions;
using FuLu.Passport.Domain.Entities;
using Fulu.Passport.Domain.Interface.Repositories;
using FuLu.Passport.Domain.Interface.Repositories;
using Fulu.Passport.Domain.Models;
using Fulu.WebAPI.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fulu.Passport.API.Controllers
{
    /// <summary>
    /// client
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController, Authorize(Roles = ClaimRoles.Client)]
    public class ClientController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IClientRepository _clientRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClientInCacheRepository _clientInCacheRepository;
        public ClientController(IClientRepository clientRepository, IMapper mapper, IUnitOfWork unitOfWork, IClientInCacheRepository clientInCacheRepository)
        {
            _clientRepository = clientRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _clientInCacheRepository = clientInCacheRepository;
        }
        /// <summary>
        /// 获取应用列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(ActionObjectResult<List<ClientEntity>, Statistic>), 200)]
        public async Task<IActionResult> GetClients()
        {
            var clients = await _clientRepository.TableNoTracking.Where(c => c.Enabled).ToListAsync();
            return ObjectResponse.Ok(clients);
        }

        /// <summary>
        /// 创建应用信息
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<IActionResult> Create([FromBody] ModifyClientInputDto input)
        {
            var clientEntity = _mapper.Map<ClientEntity>(input);
            clientEntity.Id = Guid.NewGuid().ToString();
            clientEntity.ClientSecret = Guid.NewGuid().ToString("N");
            clientEntity.Enabled = true;
            await _clientRepository.InsertAsync(clientEntity);
            await _unitOfWork.SaveChangesAsync();

            await _clientInCacheRepository.ClearCacheByIdAsync(clientEntity.ClientId);
            return ObjectResponse.Ok();
        }

        /// <summary>
        /// 修改应用信息
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ActionObjectResult), 200)]
        public async Task<IActionResult> Update(string id, [FromBody] ModifyClientInputDto input)
        {
            var clientEntity = await _clientRepository.Table.FirstOrDefaultAsync(c => c.Id == id);
            if (clientEntity != null)
            {
                _mapper.Map(input, clientEntity);

                await _unitOfWork.SaveChangesAsync();

                await _clientInCacheRepository.ClearCacheByIdAsync(clientEntity.ClientId);
            }

            return ObjectResponse.Ok();
        }

    }
}
