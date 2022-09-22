using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    //[Route("API/[Controller]")]
    [Route("API/VillaNumberAPI")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        private readonly IVillaNumberRepository _villaNumberRepository;
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;
        protected APIResponse _response;
        public VillaNumberAPIController(IVillaNumberRepository villaNumberRepository,IVillaRepository villaRepository, IMapper mapper)
        {
            _villaNumberRepository = villaNumberRepository;
            _villaRepository = villaRepository;
            _mapper = mapper;
            this._response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<APIResponse>> GetVillas()
        {
            try
            {
                _response.Result = await _villaNumberRepository.GetAllAsync();
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpGet("{id:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VillaNumberDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(VillaNumberDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(VillaNumberDTO))]

        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                    return BadRequest(_response);
                }
                var villaNumber = await _villaNumberRepository.GetAsync(v => v.VillaNo == id);

                if (villaNumber == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(VillaNumberDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(VillaNumberDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(VillaNumberDTO))]
        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (await _villaNumberRepository.GetAsync(v => v.VillaNo == createDTO.VillaNo) != null)
                {
                    ModelState.AddModelError("CustomeError", "Villa number already exist");
                    return BadRequest(ModelState);
                }
                if (await _villaRepository.GetAsync(v=>v.Id == createDTO.VillaId) == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "VillaId is invalid" };
                    return BadRequest(_response);
                }
                if (createDTO == null)
                {
                    return BadRequest(createDTO);
                }
                VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDTO);

                await _villaNumberRepository.CreateAsync(villaNumber);
                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = System.Net.HttpStatusCode.Created;
                return CreatedAtRoute("GetVillaNumber", new { id = villaNumber.VillaNo }, _response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(VillaNumberDTO))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(VillaNumberDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(VillaNumberDTO))]
        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest();
                }

                var villaNumber = await _villaNumberRepository.GetAsync(v => v.VillaNo == id);
                if (villaNumber == null)
                {
                    return NotFound();
                }

                await _villaNumberRepository.RemoveAsync(villaNumber);

                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(VillaNumberDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(VillaNumberDTO))]
        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.VillaNo)
                {
                    return BadRequest();
                }
                if (await _villaRepository.GetAsync(u=>u.Id == updateDTO.VillaId) == null)
                {
                    _response.StatusCode = System.Net.HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    _response.ErrorMessages = new List<string> { "VillaId is invalid" };
                    return BadRequest(_response);
                }
                VillaNumber villaNumber = _mapper.Map<VillaNumber>(updateDTO);

                await _villaNumberRepository.UpdateAsync(villaNumber);
                _response.StatusCode = System.Net.HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.StatusCode = System.Net.HttpStatusCode.InternalServerError;
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPatch("{id:int}", Name = "UpdatePartialVillaNumber")]
        [ProducesResponseType(StatusCodes.Status204NoContent, Type = typeof(VillaNumberDTO))]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(VillaNumberDTO))]
        public async Task<IActionResult> UpdatePartialVillaNumber(int id, JsonPatchDocument<VillaNumberUpdateDTO> patchDTO)
        {
            if (patchDTO == null || id == 0)
            {
                return BadRequest();
            }
            var villa = await _villaNumberRepository.GetAsync(v => v.VillaNo == id, tracked: false);
            VillaNumberUpdateDTO villaNumberUpdateDTO = _mapper.Map<VillaNumberUpdateDTO>(villa);

            if (villa == null)
            {
                return BadRequest();
            }
            patchDTO.ApplyTo(villaNumberUpdateDTO, ModelState);

            VillaNumber villaNumber = _mapper.Map<VillaNumber>(villaNumberUpdateDTO);

            await _villaNumberRepository.UpdateAsync(villaNumber);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return NoContent();
        }
    }
}
