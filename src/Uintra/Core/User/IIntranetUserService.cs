﻿using System.Threading.Tasks;

namespace Uintra.Core.User
{
    public interface IIntranetUserService<T>
    {
	    T GetByEmail(string email);
	    T GetById(int id);
	    void Disable(int id);
	    void Enable(int id);
	    Task<T> GetByEmailAsync(string email);
        Task<T> GetByIdAsync(int id);
	}
}
