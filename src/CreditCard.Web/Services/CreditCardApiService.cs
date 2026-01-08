namespace CreditCard.Web.Services;

using System.Net.Http.Json;
using CreditCard.Web.Models;

public class CreditCardApiService
{
    private readonly HttpClient _httpClient;

    public CreditCardApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CreditCardViewModel>> GetAllAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<PagedResponse>("api/creditcards?pageNumber=1&pageSize=1000");
        return response?.Items ?? [];
    }

    public async Task<PagedResult<CreditCardViewModel>> GetAllPagedAsync(int pageNumber = 1, int pageSize = 10)
    {
        var response = await _httpClient.GetFromJsonAsync<PagedResult<CreditCardViewModel>>(
            $"api/creditcards?pageNumber={pageNumber}&pageSize={pageSize}");
        return response ?? new PagedResult<CreditCardViewModel>();
    }

    private class PagedResponse
    {
        public List<CreditCardViewModel>? Items { get; set; }
    }

    public async Task<CreditCardViewModel?> GetByIdAsync(Guid id)
    {
        return await _httpClient.GetFromJsonAsync<CreditCardViewModel>($"api/creditcards/{id}");
    }

    public async Task<CreditCardViewModel> CreateAsync(CreateCreditCardModel model)
    {
        var response = await _httpClient.PostAsJsonAsync("api/creditcards", new
        {
            model.CardNumber,
            model.CardHolderName,
            model.ExpirationDate,
            model.CVV,
            model.CreditLimit,
            model.CardType
        });
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(error?.Error ?? "Error al crear la tarjeta");
        }
        
        return await response.Content.ReadFromJsonAsync<CreditCardViewModel>()
            ?? throw new Exception("Error al crear la tarjeta");
    }

    public async Task<CreditCardViewModel> UpdateAsync(Guid id, UpdateCreditCardModel model)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/creditcards/{id}", new
        {
            model.CardHolderName,
            model.CreditLimit
        });
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(error?.Error ?? "Error al actualizar la tarjeta");
        }
        
        return await response.Content.ReadFromJsonAsync<CreditCardViewModel>()
            ?? throw new Exception("Error al actualizar la tarjeta");
    }

    public async Task DeleteAsync(Guid id)
    {
        var response = await _httpClient.DeleteAsync($"api/creditcards/{id}");
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(error?.Error ?? "Error al eliminar la tarjeta");
        }
    }

    public async Task<CreditCardViewModel> MakeChargeAsync(Guid id, decimal amount)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/creditcards/{id}/charge", new { Amount = amount });
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(error?.Error ?? "Error al realizar el cargo");
        }
        
        return await response.Content.ReadFromJsonAsync<CreditCardViewModel>()
            ?? throw new Exception("Error al realizar el cargo");
    }

    public async Task<CreditCardViewModel> MakePaymentAsync(Guid id, decimal amount)
    {
        var response = await _httpClient.PostAsJsonAsync($"api/creditcards/{id}/payment", new { Amount = amount });
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(error?.Error ?? "Error al realizar el pago");
        }
        
        return await response.Content.ReadFromJsonAsync<CreditCardViewModel>()
            ?? throw new Exception("Error al realizar el pago");
    }

    public async Task<CreditCardViewModel> ActivateAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"api/creditcards/{id}/activate", null);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(error?.Error ?? "Error al activar la tarjeta");
        }
        
        return await response.Content.ReadFromJsonAsync<CreditCardViewModel>()
            ?? throw new Exception("Error al activar la tarjeta");
    }

    public async Task<CreditCardViewModel> DeactivateAsync(Guid id)
    {
        var response = await _httpClient.PostAsync($"api/creditcards/{id}/deactivate", null);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            throw new Exception(error?.Error ?? "Error al desactivar la tarjeta");
        }
        
        return await response.Content.ReadFromJsonAsync<CreditCardViewModel>()
            ?? throw new Exception("Error al desactivar la tarjeta");
    }

    private class ErrorResponse
    {
        public string? Error { get; set; }
    }
}
