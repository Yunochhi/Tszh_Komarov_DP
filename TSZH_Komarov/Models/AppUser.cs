using System;
using System.Collections.Generic;

namespace TSZH_Komarov.Models;

public partial class AppUser
{
    public int UserId { get; set; }

    public int Role { get; set; }

    public string Fullname { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public int IsFirstLogin { get; set; }

    public string? ChatId { get; set; }

    public virtual ICollection<Apartment> Apartments { get; set; } = new List<Apartment>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<ServiceRequest> ServiceRequests { get; set; } = new List<ServiceRequest>();

    public virtual ICollection<TopicComment> TopicComments { get; set; } = new List<TopicComment>();

    public virtual ICollection<Topic> Topics { get; set; } = new List<Topic>();

    public virtual ICollection<Vote> Votes { get; set; } = new List<Vote>();
}
