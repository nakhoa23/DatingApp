﻿using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IPhotoService _photoService;
    public UsersController(IUnitOfWork unitOfWork, IMapper mapper, IPhotoService photoService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _photoService = photoService;
    }

    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
    {
        userParams.CurrentUserName = User.GetUsername();
        var users = await _unitOfWork.UserRepository.GetMembersAsync(userParams);

        Response.AddPaginationHeader(users);

        return Ok(users);
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<MemberDto>> GetUser(string username)
    {
        var user = await _unitOfWork.UserRepository.GetMemberAsync(username.ToLower());
        if (user == null) return NotFound();
        return user;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername()); //start tracking

        if (user == null) return BadRequest("Could not find user"); 


        _mapper.Map(memberUpdateDto, user); // update run here

        if(await _unitOfWork.Complete()) return NoContent();

        return BadRequest("failed to update the user");
        
    }

    [HttpPost("add-photo")]
    public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername()); //start tracking

        if (user == null) return BadRequest("Cant update user");

        var result = await _photoService.AddPhotoAsync(file);

        if(result.Error != null) return BadRequest(result.Error.Message);

        var photo = new Photo
        {
            Url = result.SecureUrl.AbsoluteUri,
            PublicId = result.PublicId,
        };

        if(user.Photos.Count == 0) photo.isMain = true;

        user.Photos.Add(photo);

        if (await _unitOfWork.Complete()) 
            return CreatedAtAction(nameof(GetUser), new {username= user.UserName}, _mapper.Map<PhotoDto>(photo));

        return BadRequest("Problem adding photo");
    }


    [HttpPut("set-main-photo/{photoId:int}")]
    public async Task<ActionResult> SetMainPhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername());
        if (user == null) return BadRequest("Can not find user");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null || photo.isMain) return BadRequest("Can not use this as main photo");

        var currentMain = user.Photos.FirstOrDefault(x=>x.isMain);
        if (currentMain != null) currentMain.isMain = false;
        photo.isMain = true;

        if(await _unitOfWork.Complete()) return NoContent();

        return BadRequest("Problem setting main photo");
        
    }

    [HttpDelete("delete-photo/{photoId:int}")]
    public async Task<ActionResult> DeletePhoto(int photoId)
    {
        var user = await _unitOfWork.UserRepository.GetUserByUserNameAsync(User.GetUsername());
        if (user == null) return BadRequest("User not found");

        var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
        if (photo == null || photo.isMain) return BadRequest("This photo cannot be deleted");

        if(photo.PublicId != null) // couldinary
        {
            var result = await _photoService.DeletePhotoAsync(photo.PublicId);
            if(result.Error != null) return BadRequest(result.Error.Message);
        }

        user.Photos.Remove(photo);
        if (await _unitOfWork.Complete()) return Ok();

        return BadRequest("Problem deleting photo");    
    }
}
