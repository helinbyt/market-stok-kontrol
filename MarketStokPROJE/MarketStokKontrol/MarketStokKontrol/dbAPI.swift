import Foundation

struct Urun: Identifiable, Codable {
    var id: Int { urun_id }
    var urun_id: Int
    var ad: String
    var stok_adedi: Int
}


class UrunViewModel: ObservableObject {
    @Published var urunler: [Urun] = []

    func fetchUrun() async {
        guard let url = URL(string: "http://localhost:7071/api/HttpTrigger1/Urunler") else {
            print("Geçersiz URL")
            return
        }
        
        do {
            let (data, response) = try await URLSession.shared.data(from: url)

            if let httpResponse = response as? HTTPURLResponse, httpResponse.statusCode != 200 {
                print("HTTP Hata Kodu: \(httpResponse.statusCode)")
                return
            }
            
            if let jsonString = String(data: data, encoding: .utf8) {
                print("Gelen JSON: \(jsonString)")
            }

            let decodedUrunler: [Urun] = try JSONDecoder().decode([Urun].self, from: data)
            
            DispatchQueue.main.async {
                self.urunler = decodedUrunler 
            }
        } catch {
            print("Veri çekme hatası: \(error.localizedDescription)")
        }
    }
}
