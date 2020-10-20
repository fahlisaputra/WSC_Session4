using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WSC_Session4.Class;

namespace WSC_Session4
{
    public partial class WarehouseManagementForm : Form
    {
        OrderItem orderItem;
        List<OrderItem> selectedItems = new List<OrderItem>();
        List<OrderItem> removedItems = new List<OrderItem>();
        public WarehouseManagementForm(OrderItem orderItem)
        {
            InitializeComponent();
            this.orderItem = orderItem;
        }
        void initData()
        {
            var source = Global.db.Warehouses.ToArray()
                .Select(x => new
                {
                    x.ID,
                    x.Name
                }).ToArray();

            var destination = Global.db.Warehouses.ToArray()
                .Select(x => new
                {
                    x.ID,
                    x.Name
                }).ToArray();

            cbSource.ValueMember = "ID";
            cbSource.DisplayMember = "Name";
            cbSource.DataSource = source;

            cbDestination.ValueMember = "ID";
            cbDestination.DisplayMember = "Name";
            cbDestination.DataSource = destination;

            var parts = Global.db.Parts.ToArray()
                .Select(x => new
                {
                    x.ID,
                    x.Name
                }).ToArray();

            cbPart.ValueMember = "ID";
            cbPart.DisplayMember = "Name";
            cbPart.DataSource = parts;
        }

        void loadItem()
        {
            dgv.DataSource = null;
            dgv.Columns.Clear();
            var source = selectedItems.Select(x => new
            {
                x.PartID,
                x.Part.Name,
                x.BatchNumber,
                x.Amount
            }).ToArray();

            dgv.DataSource = source;
            dgv.Columns[0].Visible = false;

            var btn = new DataGridViewButtonColumn
            {
                Name = "Remove",
                HeaderText = "Action",
                Text = "Remove",
                UseColumnTextForButtonValue = true
            };
            dgv.Columns.AddRange(btn);
        }
        private void WarehouseManagementForm_Load(object sender, EventArgs e)
        {
            if (orderItem == null)
            {
                initData();
                loadItem();
            } else
            {
                if (orderItem.Order.TransactionTypeID == 2)
                {
                    var existingItems = Global.db.OrderItems.Where(a => a.OrderID == orderItem.OrderID).Select(x => new
                    {
                        x.ID,
                        x.PartID,
                        x.BatchNumber,
                        x.Amount,
                        x.Part
                    }).ToArray();

                    foreach (var item in existingItems)
                    {
                        var add = Global.db.OrderItems.Find(item.ID);
                        selectedItems.Add(add);
                    }
                    initData();
                    loadItem();

                    cbSource.SelectedValue = orderItem.Order.SourceWarehouseID;
                    cbDestination.SelectedValue = orderItem.Order.DestinationWarehouseID;
                    datePicker.Value = orderItem.Order.Date;
                }
                else
                {
                    Global.WarningAlert("Transaction type isn't valid");
                    this.Close();
                }
            }
            
        }

        private void cbPart_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id = int.Parse(cbPart.SelectedValue.ToString());

            var item = Global.db.Parts.Find(id);
            if (item.BatchNumberHasRequired == true)
            {
                cbBatch.Enabled = true;
                cbBatch.DataSource = null;

                var batch = Global.db.OrderItems
                    .Select(x => new
                    {
                        x.PartID,
                        x.BatchNumber
                    }).ToArray();

                var batchID = new List<String>();
                foreach(var order in batch)
                {
                    if (order.PartID == id)
                    {
                        if(!batchID.Contains(order.BatchNumber))
                        {
                            batchID.Add(order.BatchNumber);
                        }                     
                    }
                }

                cbBatch.DataSource = batchID;
            } else
            {
                cbBatch.DataSource = null;
                cbBatch.Enabled = false;
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
           
            if (!txtAmount.Text.isNumber())
            {
                Global.WarningAlert("Please input a valid amount");
                return;
            }

            if (int.Parse(txtAmount.Text) < 1)
            {
                Global.WarningAlert("Amount should be positive number");
                return;
            }
            int id = Convert.ToInt32(cbPart.SelectedValue);
            var part = Global.db.Parts.Find(id);
            int source = Convert.ToInt32(cbSource.SelectedValue);
            if (part == null)
            {
                Global.WarningAlert("The detail data of selected part is not found. You can close this window and open again to refresh data");
                return;
            } else
            {
                var data = Global.db.Parts.Where(x => x.ID == part.ID).Select(x => new
                {
                    current = (Decimal)(Global.db.OrderItems.Where(a => a.PartID == part.ID && a.Order.DestinationWarehouseID == source).Sum(a => (Decimal?)a.Amount)) - (Global.db.OrderItems.Where(a => a.PartID == part.ID && a.Order.SourceWarehouseID == source).Sum(a => (Decimal?)a.Amount) == null ? 0 : Global.db.OrderItems.Where(a => a.PartID == part.ID && a.Order.SourceWarehouseID == source).Sum(a => (Decimal?)a.Amount))
                }).ToArray();

                bool stock = false;
                decimal available = 0;
                foreach (var item in data)
                {
                    if (item.current >= (Convert.ToInt32(txtAmount.Text)))
                    {
                        stock = true;
                        available = item.current.Value;
                    }
                }

                if (stock == false)
                {
                    Global.ErrorAlert("There is no stocks available on selected warehouse");
                    return;
                }

                if (int.Parse(txtAmount.Text) >= part.MinimumAmount)
                {
                    String batch = cbBatch.SelectedItem == null ? "" : cbBatch.SelectedItem.ToString();
                   
                    var items = selectedItems.FirstOrDefault(x => x.PartID == part.ID && x.BatchNumber == batch);
                    
                    if (items == null)
                    {
                        items = new OrderItem();
                        items.ID = id;
                        items.BatchNumber = "";
                        items.Amount = int.Parse(txtAmount.Text);
                        items.Part = part;
                        items.PartID = part.ID;
                        if (part.BatchNumberHasRequired == true)
                        {
                            items.BatchNumber = cbBatch.SelectedItem.ToString();
                        }
                        selectedItems.Add(items);
                    } else
                    {
                        int index = selectedItems.IndexOf(items);
                        if (part.BatchNumberHasRequired == true)
                        {
                            if (selectedItems[index].BatchNumber == cbBatch.SelectedItem.ToString())
                            {
                                
                                if ((selectedItems[index].Amount + int.Parse(txtAmount.Text)) <= Convert.ToInt32(available))
                                {
                                    selectedItems[index].Amount = selectedItems[index].Amount + int.Parse(txtAmount.Text);
                                } else
                                {
                                    Global.ErrorAlert("There is no stocks available on selected warehouse");
                                }
                                
                            }
                            else
                            {
                                items = new OrderItem();
                                items.PartID = id;
                                items.BatchNumber = cbBatch.SelectedItem.ToString();
                                items.Amount = int.Parse(txtAmount.Text);
                                items.Part = part;
                                selectedItems.Add(items);
                            }
                        } else
                        {
                            
                            if ((selectedItems[index].Amount + int.Parse(txtAmount.Text)) <= Convert.ToInt32(available))
                            {
                                selectedItems[index].Amount = selectedItems[index].Amount + int.Parse(txtAmount.Text);
                            }
                            else
                            {
                                Global.ErrorAlert("There is no stocks available on selected warehouse");
                            }
                        }
                        
                        
                    }
                    loadItem();
                } else
                {
                    Global.WarningAlert("Minimum amount is " + part.MinimumAmount);
                }
            }
        }

        private void dgv_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex == -1)
            {
                return;
            }

            if (dgv.Columns[e.ColumnIndex].Name == "Remove")
            {
                int partID = int.Parse(dgv[0, e.RowIndex].Value.ToString());
                string batch = dgv[2, e.RowIndex].Value.ToString();
                var item = selectedItems.FirstOrDefault(x => x.PartID == partID && x.BatchNumber == batch);
                if (item != null)
                {
                    if (Global.ChoiceAlert("Are you sure want to remove this item?") == true)
                    {
                        removedItems.Add(item);
                        selectedItems.Remove(item);
                    }
                }
                loadItem();
            }
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (cbSource.SelectedValue.ToString() == cbDestination.SelectedValue.ToString())
            {
                Global.ErrorAlert("Source and destination warehouse cannot be same place");
                return;
            }

            if (selectedItems.Count <= 0)
            {
                Global.ErrorAlert("There is no selected parts");
                return;
            }

            if (orderItem != null)
            {
                var order = Global.db.Orders.FirstOrDefault(x => x.ID == orderItem.OrderID);
                order.SourceWarehouseID = int.Parse(cbSource.SelectedValue.ToString());
                order.DestinationWarehouseID = int.Parse(cbDestination.SelectedValue.ToString());
                order.Date = datePicker.Value.Date;
                Global.db.SaveChanges();

                foreach(var item in selectedItems)
                {
                    var exist = Global.db.OrderItems.Find(item.ID);
                    if (exist == null)
                    {
                        exist = new OrderItem();
                        exist.OrderID = order.ID;
                        exist.PartID = item.PartID;
                        exist.BatchNumber = item.BatchNumber;
                        exist.Amount = item.Amount;
                        Global.db.OrderItems.Add(exist);
                        Global.db.SaveChanges();
                    }
                }

                foreach(var item in removedItems)
                {
                    var exist = Global.db.OrderItems.Find(item.ID);
                    if (exist != null)
                    {
                        Global.db.OrderItems.Remove(exist);
                        Global.db.SaveChanges();
                    }
                }
                this.Close();

            } else
            {
                var order = new Order
                {
                    TransactionTypeID = 2,
                    SourceWarehouseID = int.Parse(cbSource.SelectedValue.ToString()),
                    DestinationWarehouseID = int.Parse(cbDestination.SelectedValue.ToString()),
                    Date = datePicker.Value.Date
                };
                Global.db.Orders.Add(order);
                Global.db.SaveChanges();

                foreach(var item in selectedItems)
                {
                    var newitems = new OrderItem
                    {
                        OrderID = order.ID,
                        PartID = item.PartID,
                        BatchNumber = item.BatchNumber,
                        Amount = item.Amount
                    };
                    Global.db.OrderItems.Add(newitems);
                    Global.db.SaveChanges();
                }

                this.Close();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
