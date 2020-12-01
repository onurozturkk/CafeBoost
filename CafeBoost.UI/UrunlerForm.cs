﻿using CafeBoost.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CafeBoost.UI
{
    public partial class UrunlerForm : Form
    {
        private readonly CafeBoostContext db;
        BindingList<Urun> blUrunler;
        public UrunlerForm(CafeBoostContext cafeBoostContext)
        {
            db = cafeBoostContext;
            blUrunler = new BindingList<Urun>(db.Urunler.ToList());
            InitializeComponent();
            dgvUrunler.AutoGenerateColumns = false;
            dgvUrunler.DataSource = blUrunler;
        }

        private void btnUrunEkle_Click(object sender, EventArgs e)
        {
            string urunAd = txtUrunAd.Text.Trim();

            if (urunAd == string.Empty)
            {
                errorProvider1.SetError(txtUrunAd, "Ürün adı girmediniz.");
                return;
            }

            if (UrunVarMi(urunAd))
            {
                errorProvider1.SetError(txtUrunAd, "Ürün zaten tanımlı.");
                return;
            }
            // hata mesajını kaldırmak için
            errorProvider1.SetError(txtUrunAd, "");

            Urun urun = new Urun()
            {
                UrunAd = urunAd,
                BirimFiyat = nudBirimFiyat.Value
            };
            db.Urunler.Add(urun);
            db.SaveChanges();
            blUrunler.Add(urun);

            txtUrunAd.Clear();
            nudBirimFiyat.Value = 0;
        }

        private void txtUrunAd_Validating(object sender, CancelEventArgs e)
        {
            if (txtUrunAd.Text.Trim() != "")
            {
                errorProvider1.SetError(txtUrunAd, "");
            }
        }

        private void dgvUrunler_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            Urun urun = (Urun)dgvUrunler.Rows[e.RowIndex].DataBoundItem;

            string mevcutDeger = dgvUrunler.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

            // Mevcut hücrede değişiklik yapılmadıysa ya da yapıldıysa ancak değer aynı kaldıysa
            if (!dgvUrunler.IsCurrentCellDirty || e.FormattedValue.ToString() == mevcutDeger)
            {
                return;
            }

            if (e.ColumnIndex == 0)
            {
                if (e.FormattedValue.ToString() == "")
                {
                    MessageBox.Show("Ürün adı boş geçilemez.");
                    e.Cancel = true;
                }

                if (BaskaUrunVarMi(e.FormattedValue.ToString(), urun))
                {
                    MessageBox.Show("Ürün zaten mevcut.");
                    e.Cancel = true;
                }
            }
            else if (e.ColumnIndex == 1)
            {
                decimal birimFiyat;
                bool gecerliMi = decimal.TryParse(e.FormattedValue.ToString().Replace("₺", ""), out birimFiyat);

                if (!gecerliMi || birimFiyat < 0)
                {
                    MessageBox.Show("Geçersiz fiyat.");
                    e.Cancel = true;
                }
            }
        }
        private bool UrunVarMi(string urunAd)
        {
            return db.Urunler.Any(
                x => x.UrunAd.Equals(urunAd, StringComparison.CurrentCultureIgnoreCase));
        }

        private bool BaskaUrunVarMi(string urunAd, Urun urun)
        {
            return db.Urunler.Any(
                x => x.UrunAd.Equals(urunAd, StringComparison.CurrentCultureIgnoreCase) && x.Id != urun.Id);
        }

        private void dgvUrunler_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            Urun urun = (Urun)e.Row.DataBoundItem;
            if (urun.SiparisDetaylar.Any())
            {
                MessageBox.Show("Seçtiğiniz ürün mevcut ya da geçmiş siparişlerde yer aldığı için silinemez");
                e.Cancel = true;
                return;
            }
            db.Urunler.Remove(urun);
            db.SaveChanges();
        }

        private void dgvUrunler_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            db.SaveChanges();
        }
    }
}
