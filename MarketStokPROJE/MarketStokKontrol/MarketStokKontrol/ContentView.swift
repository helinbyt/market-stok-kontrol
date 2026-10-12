import SwiftUI

struct ContentView: View {
    @AppStorage("girisYapildi") private var girisYapildi = false
    
    var body: some View {
        if girisYapildi {
            AnaEkranView() 
        } else {
            GirisEkraniView()
        }
    }
}

