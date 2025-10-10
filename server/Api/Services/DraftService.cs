using System.Security.Claims;
using Api.Security;
using DataAccess.Repositories;

namespace Api.Services;

public interface IDraftService
{
    Models.Dtos.Responses.DraftDetail GetById(long id);
    IEnumerable<Models.Dtos.Responses.Draft> List();
    Task<long> Create(ClaimsPrincipal claims, Models.Dtos.Requests.DraftFormData data);
    Task Update(ClaimsPrincipal claims, long id, Models.Dtos.Requests.DraftFormData data);
    Task Delete(long id);
}

public class DraftService(
    IRepository<DataAccess.Entities.Post> _postRepository,
    IRepository<DataAccess.Entities.User> _userRepository
) : IDraftService
{
    public static string[] AllowedRoles => [Role.Admin, Role.Editor];

    public Models.Dtos.Responses.DraftDetail GetById(long id)
    {
        var post = _postRepository.Query().Single(x => x.Id == id);
        var user = _userRepository.Query().Single(x => x.Id == post.AuthorId);
        return new Models.Dtos.Responses.DraftDetail(
            Id: post.Id,
            Title: post.Title,
            Content: post.Content,
            Author: new Models.Dtos.Responses.Writer(user.Id, user.UserName!)
        );
    }

    public IEnumerable<Models.Dtos.Responses.Draft> List()
    {
        return _postRepository
            .Query()
            .Where(post => post.PublishedAt == null)
            .Join(
                _userRepository.Query(),
                post => post.AuthorId,
                user => user.Id,
                (post, user) => new { post, user }
            )
            .Select(x => new Models.Dtos.Responses.Draft(
                x.post.Id,
                x.post.Title,
                new Models.Dtos.Responses.Writer(x.user.Id, x.user!.UserName!)
            ))
            .ToArray();
    }

    public async Task<long> Create(ClaimsPrincipal claims, Models.Dtos.Requests.DraftFormData data)
    {
        var currentUserId = claims.GetUserId();
        var post = new DataAccess.Entities.Post
        {
            Title = data.Title,
            Content = data.Content,
            AuthorId = currentUserId,
            PublishedAt = data.Publish ?? false ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        await _postRepository.Add(post);
        return post.Id;
    }

    public async Task Update(ClaimsPrincipal claims, long id, Models.Dtos.Requests.DraftFormData data)
    {
        var currentUserId = claims.GetUserId();
        var post = _postRepository
            .Query()
            .Where(x => x.AuthorId == currentUserId)
            .Single(x => x.Id == id);
        post.Title = data.Title;
        post.Content = data.Content;
        post.UpdatedAt = DateTime.UtcNow;
        if (data.Publish ?? false)
        {
            post.PublishedAt = DateTime.UtcNow;
        }
        await _postRepository.Update(post);
    }

    public async Task Delete(long id)
    {
        var post = _postRepository.Query().Single(x => x.Id == id);
        await _postRepository.Delete(post);
    }
}